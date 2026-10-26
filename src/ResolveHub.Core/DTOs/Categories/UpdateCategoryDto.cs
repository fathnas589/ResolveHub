namespace ResolveHub.Core.DTOs.Categories;

using System;

public class UpdateCategoryDto
{
    public Guid DepartmentGId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
