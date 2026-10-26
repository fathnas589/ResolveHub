namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Departments;
using ResolveHub.Core.Entities;

public interface IDepartmentRepository
{
    Task<Department?> InsertAsync(Department department);
    Task<Department?> UpdateAsync(Department department);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<Department?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<Department>> GetListingAsync(int companyId, DepartmentListRequestDto request);
    Task<bool> CheckDuplicateNameAsync(int companyId, string departmentName, Guid? excludeGId = null);
}
