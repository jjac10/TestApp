using Microsoft.AspNetCore.Mvc;
using TestApp.Api.DTOs;
using TestApp.Core.Services;

namespace TestApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token, user, role, error) = await _authService.LoginAsync(request.Email, request.Password);

        if (!success)
            return Unauthorized(new { error });

        return Ok(new LoginResponse(
            token,
            user!.Email!,
            user.FullName,
            role,
            DateTime.UtcNow.AddHours(24)));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var (success, user, error) = await _authService.RegisterAsync(request.Email, request.Password, request.FullName);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Usuario registrado correctamente", email = user!.Email });
    }
}