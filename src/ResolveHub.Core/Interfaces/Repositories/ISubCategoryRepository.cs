namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.SubCategories;
using ResolveHub.Core.Entities;

public interface ISubCategoryRepository
{
    Task<SubCategory?> InsertAsync(SubCategory subCategory);
    Task<SubCategory?> UpdateAsync(SubCategory subCategory);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<SubCategory?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<SubCategory>> GetListingAsync(int companyId, SubCategoryListRequestDto request);
    Task<bool> CheckDuplicateNameAsync(int companyId, Guid categoryGId, string subCategoryName, Guid? excludeGId = null);
}
