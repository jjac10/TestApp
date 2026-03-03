using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestApp.Api.DTOs;
using TestApp.Core.Services;

namespace TestApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;

    public AdminController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var (success, user, error) = await _authService.RegisterAsync(
            request.Email, request.Password, request.FullName, request.Role);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Usuario creado correctamente", email = user!.Email, role = request.Role });
    }
}