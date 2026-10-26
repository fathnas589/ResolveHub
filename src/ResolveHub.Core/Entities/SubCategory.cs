namespace ResolveHub.Core.Entities;

using System;

public class SubCategory
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public int CategoryId { get; set; }
    public Guid CategoryGId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid DepartmentGId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CompanyId { get; set; }
    public DateTime Created { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? LastUpd { get; set; }
    public int? LastUpdBy { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}
