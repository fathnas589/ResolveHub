namespace ResolveHub.Core.DTOs.Categories;

using System;

public class CategoryResponseDto
{
    public Guid GId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid DepartmentGId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
}
