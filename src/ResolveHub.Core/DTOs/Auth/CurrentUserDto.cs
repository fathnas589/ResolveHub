namespace ResolveHub.Core.DTOs.Auth;

using System;

public class CurrentUserDto
{
    public Guid UserGId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int CompanyId { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid? DepartmentGId { get; set; }
    public string? DepartmentName { get; set; }
}
