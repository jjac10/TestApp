namespace TestApp.Api.DTOs;

public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string Password);
public record LoginResponse(string Token, string Email, string FullName, string Role, DateTime Expiration);
public record CreateUserRequest(string Email, string Password, string FullName, string Role = "User");