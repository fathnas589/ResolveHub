namespace ResolveHub.Services.Interfaces;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.SubCategories;

public interface ISubCategoryService
{
    Task<SubCategoryResponseDto> CreateAsync(int companyId, int createdBy, CreateSubCategoryDto request);
    Task<SubCategoryResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateSubCategoryDto request);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<SubCategoryResponseDto?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<SubCategoryResponseDto>> GetListingAsync(int companyId, SubCategoryListRequestDto request);
}
