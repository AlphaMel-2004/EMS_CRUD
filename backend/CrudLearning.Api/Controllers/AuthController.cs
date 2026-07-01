using CrudLearning.Api.DTOs.Auth;
using CrudLearning.Api.Helpers;
using CrudLearning.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrudLearning.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        return Ok(await _authService.LoginAsync(request));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var userId = Permissions.GetUserId(User);
        if (userId is null)
        {
            return Unauthorized(new { message = "User account not found." });
        }

        return Ok(await _authService.GetCurrentUserAsync(userId.Value));
    }
}
