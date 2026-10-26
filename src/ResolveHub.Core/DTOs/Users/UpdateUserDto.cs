namespace ResolveHub.Core.DTOs.Users;

using System;

public class UpdateUserDto
{
    public Guid? DepartmentGId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequest : UpdateUserDto
{
}
