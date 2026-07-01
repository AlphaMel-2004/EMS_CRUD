using BCrypt.Net;
using CrudLearning.Api.Controllers;
using CrudLearning.Api.Data;
using CrudLearning.Api.DTOs.Auth;
using CrudLearning.Api.DTOs.Employees;
using CrudLearning.Api.Middleware;
using CrudLearning.Api.Models;
using CrudLearning.Api.Security;
using CrudLearning.Api.Services;
using CrudLearning.Api.Services.Attendance;
using CrudLearning.Api.Services.Audit;
using CrudLearning.Api.Services.Auth;
using CrudLearning.Api.Services.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CrudLearning.Api.Tests;

public class ServiceTests
{
    [Fact]
    public async Task Login_success_returns_token()
    {
        await using var db = CreateDb();
        var employee = await SeedEmployeeAsync(db);
        db.Users.Add(new AppUser
        {
            Username = "employee",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!", workFactor: 4),
            Role = UserRole.Employee,
            EmployeeId = employee.Id,
            Employee = employee,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var service = CreateAuthService(db);
        var result = await service.LoginAsync(new LoginRequest { Username = "employee", Password = "Employee123!" });

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("Employee", result.Role);
    }

    [Fact]
    public async Task Login_failure_rejects_bad_password()
    {
        await using var db = CreateDb();
        db.Users.Add(new AppUser
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", workFactor: 4),
            Role = UserRole.Admin,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var service = CreateAuthService(db);
        var error = await Assert.ThrowsAsync<ApiException>(() =>
            service.LoginAsync(new LoginRequest { Username = "admin", Password = "Wrong123!" }));

        Assert.Equal(StatusCodes.Status401Unauthorized, error.StatusCode);
    }

    [Fact]
    public async Task Admin_can_create_employee()
    {
        await using var db = CreateDb();
        var service = CreateEmployeeService(db);

        var employee = await service.CreateEmployeeAsync(new CreateEmployeeRequest
        {
            FullName = "Mina Park",
            Email = "mina@example.com",
            Department = "Operations",
            Position = "Coordinator",
            Username = "mina",
            Password = "Employee123!"
        }, actorUserId: 1);

        Assert.Equal("Mina Park", employee.FullName);
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Fact]
    public async Task Check_in_creates_attendance_record()
    {
        await using var db = CreateDb();
        var employee = await SeedEmployeeAsync(db);
        var service = CreateAttendanceService(db);

        var result = await service.CheckInAsync(employee.Id, actorUserId: 2);

        Assert.Equal(AttendanceState.CheckedIn, result.AttendanceState);
        Assert.Equal(AttendanceEventType.CheckIn, (await db.AttendanceEntries.SingleAsync()).EventType);
    }

    [Fact]
    public async Task Check_out_updates_attendance_correctly()
    {
        await using var db = CreateDb();
        var employee = await SeedEmployeeAsync(db, AttendanceState.CheckedIn);
        var service = CreateAttendanceService(db);

        var result = await service.CheckOutAsync(employee.Id, actorUserId: 2);

        Assert.Equal(AttendanceState.CheckedOut, result.AttendanceState);
        Assert.Equal(AttendanceEventType.CheckOut, (await db.AttendanceEntries.SingleAsync()).EventType);
    }

    [Fact]
    public async Task Soft_deleted_employee_cannot_log_in()
    {
        await using var db = CreateDb();
        var employee = await SeedEmployeeAsync(db);
        employee.IsDeleted = true;
        db.Users.Add(new AppUser
        {
            Username = "deleted",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee123!", workFactor: 4),
            Role = UserRole.Employee,
            EmployeeId = employee.Id,
            Employee = employee,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var service = CreateAuthService(db);
        var error = await Assert.ThrowsAsync<ApiException>(() =>
            service.LoginAsync(new LoginRequest { Username = "deleted", Password = "Employee123!" }));

        Assert.Equal(StatusCodes.Status403Forbidden, error.StatusCode);
    }

    [Fact]
    public void Non_admin_cannot_list_or_delete_employees()
    {
        var getAuthorize = typeof(EmployeesController).GetMethod(nameof(EmployeesController.GetEmployees))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single();
        var deleteAuthorize = typeof(EmployeesController).GetMethod(nameof(EmployeesController.SoftDeleteEmployee))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal(RoleNames.Admin, getAuthorize.Roles);
        Assert.Equal(RoleNames.Admin, deleteAuthorize.Roles);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<Employee> SeedEmployeeAsync(AppDbContext db, AttendanceState attendanceState = AttendanceState.CheckedOut)
    {
        var employee = new Employee
        {
            FullName = "Ava Johnson",
            Email = $"{Guid.NewGuid():N}@example.com",
            Department = "Operations",
            Position = "Specialist",
            WorkStatus = EmployeeWorkStatus.Working,
            AttendanceState = attendanceState,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return employee;
    }

    private static AuthService CreateAuthService(AppDbContext db)
    {
        var settings = new JwtSettings { Key = "test-secret-key-with-enough-length-123456789" };
        return new AuthService(db, new JwtTokenService(settings), CreateAuditService(db));
    }

    private static EmployeeService CreateEmployeeService(AppDbContext db)
    {
        return new EmployeeService(db, CreateAuditService(db));
    }

    private static AttendanceService CreateAttendanceService(AppDbContext db)
    {
        return new AttendanceService(db, CreateAuditService(db));
    }

    private static AuditLogService CreateAuditService(AppDbContext db)
    {
        return new AuditLogService(db, new HttpContextAccessor { HttpContext = new DefaultHttpContext() });
    }
}
