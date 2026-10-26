namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Categories;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.Entities;

public interface ICategoryRepository
{
    Task<Category?> InsertAsync(Category category);
    Task<Category?> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<Category?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<Category>> GetListingAsync(int companyId, CategoryListRequestDto request);
    Task<bool> CheckDuplicateNameAsync(int companyId, Guid departmentGId, string categoryName, Guid? excludeGId = null);
}
