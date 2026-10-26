namespace ResolveHub.Core.Entities;

using System;

public class UserMaster
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public int CompanyId { get; set; }
    public int? DepartmentId { get; set; }
    public Guid? DepartmentGId { get; set; }
    public string? DepartmentName { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime Created { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastUpd { get; set; }
    public int? LastUpdBy { get; set; }
}
