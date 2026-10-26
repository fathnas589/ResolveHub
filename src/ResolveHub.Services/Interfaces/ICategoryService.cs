namespace ResolveHub.Services.Interfaces;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Categories;
using ResolveHub.Core.DTOs.Common;

public interface ICategoryService
{
    Task<CategoryResponseDto> CreateAsync(int companyId, int createdBy, CreateCategoryDto request);
    Task<CategoryResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateCategoryDto request);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<CategoryResponseDto?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<CategoryResponseDto>> GetListingAsync(int companyId, CategoryListRequestDto request);
}
