using System.ComponentModel.DataAnnotations;
using CrudLearning.Api.Models;

namespace CrudLearning.Api.DTOs.Auth;

public sealed class LoginRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public sealed record LoginResponse(
    string Token,
    string Role,
    int UserId,
    string Username,
    int? EmployeeId,
    string? FullName,
    string? Email);

public sealed record CurrentUserResponse(
    string Role,
    int UserId,
    string Username,
    int? EmployeeId,
    string? FullName,
    string? Email,
    EmployeeWorkStatus? WorkStatus,
    AttendanceState? AttendanceState,
    bool IsDeleted);
