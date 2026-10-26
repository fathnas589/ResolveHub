namespace ResolveHub.Core.DTOs.Auth;

public class LoginRequestDto
{
    public int CompanyId { get; set; }
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest : LoginRequestDto
{
}
