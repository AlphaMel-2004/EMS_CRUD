using System.Security.Claims;
using BCrypt.Net;
using CrudLearning.Api.Contracts;
using CrudLearning.Api.Data;
using CrudLearning.Api.Models;
using CrudLearning.Api.Security;
using CrudLearning.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrudLearning.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _tokenService;

    public AuthController(AppDbContext db, JwtTokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(appUser => appUser.Employee)
            .FirstOrDefaultAsync(appUser => appUser.Username.ToLower() == normalizedUsername);

        if (user is null || !user.IsActive || (user.Role == UserRole.Employee && user.Employee?.IsDeleted == true) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = _tokenService.CreateToken(user);

        return Ok(new LoginResponse(
            token,
            user.Role.ToString(),
            user.Id,
            user.Username,
            user.EmployeeId,
            user.Employee?.FullName,
            user.Employee?.Email
        ));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var user = await _db.Users
            .Include(appUser => appUser.Employee)
            .FirstOrDefaultAsync(appUser => appUser.Id == userId);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new CurrentUserResponse(
            user.Role.ToString(),
            user.Id,
            user.Username,
            user.EmployeeId,
            user.Employee?.FullName,
            user.Employee?.Email,
            user.Employee?.WorkStatus,
            user.Employee?.AttendanceState,
            user.Employee?.IsDeleted ?? false
        ));
    }
}