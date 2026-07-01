using System.Net;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Middleware;

public sealed record ApiErrorResponse(string Message, string? Details = null);

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException exception)
        {
            await WriteErrorAsync(context, exception.StatusCode, exception.Message, exception.Details);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Database update failed.");
            await WriteErrorAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                "Database is unavailable.",
                _environment.IsDevelopment() ? exception.GetBaseException().Message : null);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled API error.");
            await WriteErrorAsync(
                context,
                (int)HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                _environment.IsDevelopment() ? exception.Message : null);
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message, string? details)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new ApiErrorResponse(message, details));
    }
}
