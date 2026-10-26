namespace ResolveHub.Services.Interfaces;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Users;

public interface IUserService
{
    Task<UserResponseDto> CreateAsync(int companyId, int createdBy, CreateUserDto request);
    Task<UserResponseDto?> UpdateAsync(int companyId, Guid gId, int lastUpdBy, UpdateUserDto request);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<UserResponseDto?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<UserResponseDto>> GetListingAsync(int companyId, UserListRequestDto request);
    Task<bool> ChangePasswordAsync(int companyId, Guid gId, int lastUpdBy, ChangePasswordDto request);
}
