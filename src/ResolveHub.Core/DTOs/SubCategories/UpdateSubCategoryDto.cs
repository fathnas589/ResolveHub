namespace ResolveHub.Core.DTOs.SubCategories;

using System;

public class UpdateSubCategoryDto
{
    public Guid CategoryGId { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

// Request alias to match alternate naming conventions
public class UpdateSubCategoryRequest : UpdateSubCategoryDto
{
}
