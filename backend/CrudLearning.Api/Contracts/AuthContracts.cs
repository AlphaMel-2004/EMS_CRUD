using CrudLearning.Api.Models;

namespace CrudLearning.Api.Contracts;

public record LoginRequest(string Username, string Password);

public record LoginResponse(
    string Token,
    string Role,
    int UserId,
    string Username,
    int? EmployeeId,
    string? FullName,
    string? Email
);

public record CurrentUserResponse(
    string Role,
    int UserId,
    string Username,
    int? EmployeeId,
    string? FullName,
    string? Email,
    EmployeeWorkStatus? WorkStatus,
    AttendanceState? AttendanceState,
    bool IsDeleted
);

public sealed class CreateEmployeeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class UpdateEmployeeRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
}

public record UpdateStatusRequest(EmployeeWorkStatus WorkStatus);

public record AttendanceEntryResponse(
    int Id,
    int EmployeeId,
    string EmployeeName,
    AttendanceEventType EventType,
    DateTime OccurredAt,
    string? Note
);

public record EmployeeResponse(
    int Id,
    string FullName,
    string Email,
    string Department,
    string Position,
    string Username,
    UserRole Role,
    EmployeeWorkStatus WorkStatus,
    AttendanceState AttendanceState,
    bool IsDeleted,
    DateTime? LastCheckInAt,
    DateTime? LastCheckOutAt,
    DateTime CreatedAt,
    DateTime UpdatedAt
);