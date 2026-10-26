namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Users;
using ResolveHub.Core.Entities;

public interface IUserRepository
{
    Task<UserMaster?> InsertAsync(UserMaster user);
    Task<UserMaster?> UpdateAsync(UserMaster user);
    Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy);
    Task<UserMaster?> GetByGIdAsync(int companyId, Guid gId);
    Task<PagedResult<UserMaster>> GetListingAsync(int companyId, UserListRequestDto request);
    Task<UserLoginRecord?> GetForLoginAsync(int companyId, string userNameOrEmail);
    Task<(bool isDuplicateUserName, bool isDuplicateEmail)> CheckDuplicateAsync(int companyId, string? userName, string? email, Guid? excludeGId = null);
    Task<bool> UpdatePasswordAsync(int companyId, Guid gId, string passwordHash, int lastUpdBy);
}
