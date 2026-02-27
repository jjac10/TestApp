using TestApp.Core.Models;

namespace TestApp.Core.Services;

public interface IAuthService
{
    Task<(bool Success, string Token, User? User, string Role, string? Error)> LoginAsync(string email, string password);
    Task<(bool Success, User? User, string? Error)> RegisterAsync(string email, string password, string fullName, string role = "User");
}