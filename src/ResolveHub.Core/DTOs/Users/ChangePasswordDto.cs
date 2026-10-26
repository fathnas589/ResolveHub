namespace ResolveHub.Core.DTOs.Users;

public class ChangePasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordRequest : ChangePasswordDto
{
}
