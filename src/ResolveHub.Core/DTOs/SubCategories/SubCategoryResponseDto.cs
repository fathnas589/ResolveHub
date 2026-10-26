namespace ResolveHub.Core.DTOs.SubCategories;

using System;

public class SubCategoryResponseDto
{
    public Guid GId { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryGId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid DepartmentGId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
}

// Response alias to match alternate naming conventions
public class SubCategoryResponse : SubCategoryResponseDto
{
}
