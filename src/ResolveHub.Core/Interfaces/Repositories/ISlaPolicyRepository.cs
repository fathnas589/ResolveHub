namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Sla;
using ResolveHub.Core.Entities;

public interface ISlaPolicyRepository
{
    Task<SlaPolicy?> InsertAsync(SlaPolicy policy);
    Task<SlaPolicy?> UpdateAsync(SlaPolicy policy);
    Task<bool> DeleteAsync(int companyId, Guid gId, int deletedBy);
    Task<SlaPolicy?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<SlaPolicy>> GetListingAsync(int companyId, SlaPolicyListRequestDto request);
    Task<bool> CheckDuplicateNameAsync(int companyId, string name, Guid? excludeGId = null);
    Task<SlaPolicy?> GetBestMatchAsync(int companyId, int? departmentId, int? categoryId, int? subCategoryId, string? priority, string? ticketType);
}
