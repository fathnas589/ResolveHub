namespace ResolveHub.Core.DTOs.SubCategories;

using System;

public class SubCategoryListRequestDto
{
    public Guid? CategoryGId { get; set; }
    public Guid? DepartmentGId { get; set; }
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
