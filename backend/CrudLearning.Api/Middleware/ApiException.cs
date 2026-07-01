namespace CrudLearning.Api.Middleware;

public sealed class ApiException : Exception
{
    public ApiException(string message, int statusCode = StatusCodes.Status400BadRequest, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }
    public string? Details { get; }
}
