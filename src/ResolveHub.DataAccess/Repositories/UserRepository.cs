namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Users;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public UserRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserMaster?> InsertAsync(UserMaster user)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", user.CompanyId, DbType.Int32);
        parameters.Add("@DepartmentGId", user.DepartmentGId, DbType.Guid);
        parameters.Add("@UserName", user.UserName, DbType.String, size: 100);
        parameters.Add("@FullName", user.FullName, DbType.String, size: 150);
        parameters.Add("@Email", user.Email, DbType.String, size: 150);
        parameters.Add("@Phone", user.Phone, DbType.String, size: 30);
        parameters.Add("@PasswordHash", user.PasswordHash, DbType.String, size: 500);
        parameters.Add("@RoleCode", user.RoleCode, DbType.String, size: 50);
        parameters.Add("@IsActive", user.IsActive, DbType.Boolean);
        parameters.Add("@CreatedBy", user.CreatedBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<UserMaster>(
            "dbo.UserMaster_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<UserMaster?> UpdateAsync(UserMaster user)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", user.CompanyId, DbType.Int32);
        parameters.Add("@GId", user.GId, DbType.Guid);
        parameters.Add("@DepartmentGId", user.DepartmentGId, DbType.Guid);
        parameters.Add("@UserName", user.UserName, DbType.String, size: 100);
        parameters.Add("@FullName", user.FullName, DbType.String, size: 150);
        parameters.Add("@Email", user.Email, DbType.String, size: 150);
        parameters.Add("@Phone", user.Phone, DbType.String, size: 30);
        parameters.Add("@RoleCode", user.RoleCode, DbType.String, size: 50);
        parameters.Add("@IsActive", user.IsActive, DbType.Boolean);
        parameters.Add("@LastUpdBy", user.LastUpdBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<UserMaster>(
            "dbo.UserMaster_Update",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid gId, int lastUpdBy)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);
        parameters.Add("@LastUpdBy", lastUpdBy, DbType.Int32);

        return await connection.ExecuteScalarAsync<bool>(
            "dbo.UserMaster_Delete",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<UserMaster?> GetByGIdAsync(int companyId, Guid gId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<UserMaster>(
            "dbo.UserMaster_Get",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PagedResult<UserMaster>> GetListingAsync(int companyId, UserListRequestDto request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@DepartmentGId", request.DepartmentGId, DbType.Guid);
        parameters.Add("@RoleCode", request.RoleCode, DbType.String, size: 50);
        parameters.Add("@SearchTerm", request.SearchTerm, DbType.String, size: 100);
        parameters.Add("@IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("@PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("@PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<UserMasterListingDbRow>(
            "dbo.UserMaster_Listing",
            parameters,
            commandType: CommandType.StoredProcedure)).ToList();

        int totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new PagedResult<UserMaster>
        {
            Items = rows.Cast<UserMaster>().ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<UserLoginRecord?> GetForLoginAsync(int companyId, string userNameOrEmail)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@UserNameOrEmail", userNameOrEmail, DbType.String, size: 150);

        return await connection.QuerySingleOrDefaultAsync<UserLoginRecord>(
            "dbo.UserMaster_GetForLogin",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(bool isDuplicateUserName, bool isDuplicateEmail)> CheckDuplicateAsync(
        int companyId, string? userName, string? email, Guid? excludeGId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@UserName", userName, DbType.String, size: 100);
        parameters.Add("@Email", email, DbType.String, size: 150);
        parameters.Add("@ExcludeGId", excludeGId, DbType.Guid);

        var row = await connection.QuerySingleOrDefaultAsync<DuplicateCheckDbRow>(
            "dbo.UserMaster_DuplicateCheck",
            parameters,
            commandType: CommandType.StoredProcedure);

        return (row?.IsDuplicateUserName ?? false, row?.IsDuplicateEmail ?? false);
    }

    public async Task<bool> UpdatePasswordAsync(int companyId, Guid gId, string passwordHash, int lastUpdBy)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);
        parameters.Add("@PasswordHash", passwordHash, DbType.String, size: 500);
        parameters.Add("@LastUpdBy", lastUpdBy, DbType.Int32);

        return await connection.ExecuteScalarAsync<bool>(
            "dbo.UserMaster_UpdatePassword",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    private sealed class UserMasterListingDbRow : UserMaster
    {
        public int TotalCount { get; set; }
    }

    private sealed class DuplicateCheckDbRow
    {
        public bool IsDuplicateUserName { get; set; }
        public bool IsDuplicateEmail { get; set; }
    }
}
