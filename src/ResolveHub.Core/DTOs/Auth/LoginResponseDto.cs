namespace ResolveHub.Core.DTOs.Auth;

using System;
using ResolveHub.Core.DTOs.Users;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserResponseDto User { get; set; } = new();
}

public class LoginResponse : LoginResponseDto
{
}
