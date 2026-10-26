namespace ResolveHub.Services.Interfaces;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Sla;

public interface ISlaPolicyService
{
    Task<SlaPolicyResponseDto> CreateAsync(int companyId, int createdBy, CreateSlaPolicyDto request);
    Task<SlaPolicyResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateSlaPolicyDto request);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<SlaPolicyResponseDto?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<SlaPolicyResponseDto>> GetListingAsync(int companyId, SlaPolicyListRequestDto request);
}
