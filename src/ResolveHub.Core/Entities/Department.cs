namespace ResolveHub.Core.Entities;

using System;

public class Department
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CompanyId { get; set; }
    public DateTime Created { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? LastUpd { get; set; }
    public int? LastUpdBy { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}
