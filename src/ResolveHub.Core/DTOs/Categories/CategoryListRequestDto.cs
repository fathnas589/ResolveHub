namespace ResolveHub.Core.DTOs.Categories;

using System;

public class CategoryListRequestDto
{
    public Guid? DepartmentGId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
