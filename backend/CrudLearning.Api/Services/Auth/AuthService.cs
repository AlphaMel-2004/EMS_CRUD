using BCrypt.Net;
using CrudLearning.Api.Data;
using CrudLearning.Api.DTOs.Auth;
using CrudLearning.Api.Middleware;
using CrudLearning.Api.Models;
using CrudLearning.Api.Services.Audit;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Services.Auth;

public sealed class AuthService
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _db;
    private readonly JwtTokenService _tokenService;
    private readonly AuditLogService _auditLogService;

    public AuthService(AppDbContext db, JwtTokenService tokenService, AuditLogService auditLogService)
    {
        _db = db;
        _tokenService = tokenService;
        _auditLogService = auditLogService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(appUser => appUser.Employee)
            .FirstOrDefaultAsync(appUser => appUser.Username.ToLower() == normalizedUsername);

        if (user is null)
        {
            await _auditLogService.RecordAsync("LoginFailure", null, null, $"Unknown username attempted login: {normalizedUsername}");
            throw new ApiException("Invalid username or password.", StatusCodes.Status401Unauthorized);
        }

        if (user.LockedUntil is not null && user.LockedUntil > DateTime.UtcNow)
        {
            await _auditLogService.RecordAsync("LoginFailure", user.Id, user.EmployeeId, "Login blocked because the account is temporarily locked.");
            throw new ApiException("Too many failed login attempts. Try again later.", StatusCodes.Status423Locked);
        }

        if (!user.IsActive || (user.Role == UserRole.Employee && user.Employee?.IsDeleted == true))
        {
            await _auditLogService.RecordAsync("LoginFailure", user.Id, user.EmployeeId, "Inactive or deleted employee attempted login.");
            throw new ApiException("Deleted employees cannot log in.", StatusCodes.Status403Forbidden);
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts += 1;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
            }

            await _db.SaveChangesAsync();
            await _auditLogService.RecordAsync("LoginFailure", user.Id, user.EmployeeId, "Password is incorrect.");
            throw new ApiException("Invalid username or password.", StatusCodes.Status401Unauthorized);
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        await _db.SaveChangesAsync();

        var token = _tokenService.CreateToken(user);
        await _auditLogService.RecordAsync("LoginSuccess", user.Id, user.EmployeeId, "User logged in successfully.");

        return new LoginResponse(
            token,
            user.Role.ToString(),
            user.Id,
            user.Username,
            user.EmployeeId,
            user.Employee?.FullName,
            user.Employee?.Email);
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(int userId)
    {
        var user = await _db.Users
            .Include(appUser => appUser.Employee)
            .FirstOrDefaultAsync(appUser => appUser.Id == userId);

        if (user is null)
        {
            throw new ApiException("User account not found.", StatusCodes.Status401Unauthorized);
        }

        return new CurrentUserResponse(
            user.Role.ToString(),
            user.Id,
            user.Username,
            user.EmployeeId,
            user.Employee?.FullName,
            user.Employee?.Email,
            user.Employee?.WorkStatus,
            user.Employee?.AttendanceState,
            user.Employee?.IsDeleted ?? false);
    }
}
