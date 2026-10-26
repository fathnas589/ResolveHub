namespace ResolveHub.Core.DTOs.Users;

using System;

public class UserResponseDto
{
    public Guid GId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string Role => RoleCode;
    public Guid? DepartmentGId { get; set; }
    public string? DepartmentName { get; set; }
    public int CompanyId { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
}

public class UserResponse : UserResponseDto
{
}
