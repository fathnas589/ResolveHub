namespace ResolveHub.Services.Interfaces;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Departments;

public interface IDepartmentService
{
    Task<DepartmentResponseDto> CreateAsync(int companyId, int createdBy, CreateDepartmentDto request);
    Task<DepartmentResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateDepartmentDto request);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<DepartmentResponseDto?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<DepartmentResponseDto>> GetListingAsync(int companyId, DepartmentListRequestDto request);
}
