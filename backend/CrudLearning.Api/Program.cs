using CrudLearning.Api.Data;
using CrudLearning.Api.Middleware;
using CrudLearning.Api.Security;
using CrudLearning.Api.Services;
using CrudLearning.Api.Services.Attendance;
using CrudLearning.Api.Services.Audit;
using CrudLearning.Api.Services.Auth;
using CrudLearning.Api.Services.Employees;
using CrudLearning.Api.Services.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var details = string.Join(" ", context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .SelectMany(item => item.Value!.Errors.Select(error => error.ErrorMessage)));

        return new BadRequestObjectResult(new ApiErrorResponse("Validation failed.", details));
    };
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var jwtKeyFromEnvironment = builder.Configuration["JWT_SECRET"];
if (!string.IsNullOrWhiteSpace(jwtKeyFromEnvironment))
{
    jwtSettings.Key = jwtKeyFromEnvironment;
}

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<AuditLogService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
              uri.Host == "localhost" &&
              uri.Port is >= 5173 and <= 5199)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Configuration.GetValue<bool>("ResetDatabaseOnStartup"))
    {
        app.Logger.LogWarning("ResetDatabaseOnStartup is enabled. This is only safe for local development/demo data.");
        await dbContext.Database.EnsureDeletedAsync();
    }

    dbContext.Database.Migrate();
    await DbSeeder.SeedAsync(dbContext, scope.ServiceProvider.GetRequiredService<JwtSettings>());

    app.MapOpenApi();
}

// Configure the HTTP request pipeline.
app.UseCors("AllowReact");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
