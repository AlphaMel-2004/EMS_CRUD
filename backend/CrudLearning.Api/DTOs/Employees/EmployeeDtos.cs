using System.ComponentModel.DataAnnotations;
using CrudLearning.Api.Models;

namespace CrudLearning.Api.DTOs.Employees;

public sealed class EmployeeQuery
{
    public string? Search { get; set; }
    public string? Department { get; set; }
    public EmployeeWorkStatus? WorkStatus { get; set; }
    public AttendanceState? AttendanceState { get; set; }
    public bool IncludeDeleted { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class CreateEmployeeRequest
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Position { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public sealed class UpdateEmployeeRequest
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Position { get; set; } = string.Empty;
}

public sealed record UpdateStatusRequest(EmployeeWorkStatus WorkStatus);

public sealed record EmployeeResponse(
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
    DateTime UpdatedAt);
