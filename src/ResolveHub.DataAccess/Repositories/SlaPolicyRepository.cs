namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Sla;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class SlaPolicyRepository : ISlaPolicyRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SlaPolicyRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SlaPolicy?> InsertAsync(SlaPolicy policy)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", policy.CompanyId, DbType.Int32);
        parameters.Add("@Name", policy.Name, DbType.String, size: 100);
        parameters.Add("@Description", policy.Description, DbType.String, size: 500);
        parameters.Add("@DepartmentGId", policy.DepartmentGId, DbType.Guid);
        parameters.Add("@CategoryGId", policy.CategoryGId, DbType.Guid);
        parameters.Add("@SubCategoryGId", policy.SubCategoryGId, DbType.Guid);
        parameters.Add("@Priority", policy.Priority, DbType.String, size: 20);
        parameters.Add("@TicketType", policy.TicketType, DbType.String, size: 30);
        parameters.Add("@FirstResponseMinutes", policy.FirstResponseMinutes, DbType.Int32);
        parameters.Add("@ResolutionMinutes", policy.ResolutionMinutes, DbType.Int32);
        parameters.Add("@EscalationBeforeMinutes", policy.EscalationBeforeMinutes, DbType.Int32);
        parameters.Add("@IsDefault", policy.IsDefault, DbType.Boolean);
        parameters.Add("@IsActive", policy.IsActive, DbType.Boolean);
        parameters.Add("@CreatedBy", policy.CreatedBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<SlaPolicy>(
            "dbo.SlaPolicy_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<SlaPolicy?> UpdateAsync(SlaPolicy policy)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", policy.CompanyId, DbType.Int32);
        parameters.Add("@GId", policy.GId, DbType.Guid);
        parameters.Add("@Name", policy.Name, DbType.String, size: 100);
        parameters.Add("@Description", policy.Description, DbType.String, size: 500);
        parameters.Add("@DepartmentGId", policy.DepartmentGId, DbType.Guid);
        parameters.Add("@CategoryGId", policy.CategoryGId, DbType.Guid);
        parameters.Add("@SubCategoryGId", policy.SubCategoryGId, DbType.Guid);
        parameters.Add("@Priority", policy.Priority, DbType.String, size: 20);
        parameters.Add("@TicketType", policy.TicketType, DbType.String, size: 30);
        parameters.Add("@FirstResponseMinutes", policy.FirstResponseMinutes, DbType.Int32);
        parameters.Add("@ResolutionMinutes", policy.ResolutionMinutes, DbType.Int32);
        parameters.Add("@EscalationBeforeMinutes", policy.EscalationBeforeMinutes, DbType.Int32);
        parameters.Add("@IsDefault", policy.IsDefault, DbType.Boolean);
        parameters.Add("@IsActive", policy.IsActive, DbType.Boolean);
        parameters.Add("@LastUpdBy", policy.LastUpdBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<SlaPolicy>(
            "dbo.SlaPolicy_Update",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid gId, int deletedBy)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);
        parameters.Add("@DeletedBy", deletedBy, DbType.Int32);

        int rows = await connection.ExecuteScalarAsync<int>(
            "dbo.SlaPolicy_Delete",
            parameters,
            commandType: CommandType.StoredProcedure);

        return rows > 0;
    }

    public async Task<SlaPolicy?> GetByGIdAsync(int companyId, Guid gId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<SlaPolicy>(
            "dbo.SlaPolicy_Get",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> CheckDuplicateNameAsync(int companyId, string name, Guid? excludeGId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@Name", name, DbType.String, size: 100);
        parameters.Add("@ExcludeGId", excludeGId, DbType.Guid);

        return await connection.ExecuteScalarAsync<bool>(
            "dbo.SlaPolicy_DuplicateCheck",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PagedResult<SlaPolicy>> GetListingAsync(int companyId, SlaPolicyListRequestDto request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@SearchTerm", request.SearchTerm, DbType.String, size: 100);
        parameters.Add("@DepartmentGId", request.DepartmentGId, DbType.Guid);
        parameters.Add("@CategoryGId", request.CategoryGId, DbType.Guid);
        parameters.Add("@Priority", request.Priority, DbType.String, size: 20);
        parameters.Add("@TicketType", request.TicketType, DbType.String, size: 30);
        parameters.Add("@IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("@PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("@PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<SlaPolicyListingDbRow>(
            "dbo.SlaPolicy_Listing",
            parameters,
            commandType: CommandType.StoredProcedure)).ToList();

        int totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new PagedResult<SlaPolicy>
        {
            Items = rows.Cast<SlaPolicy>().ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<SlaPolicy?> GetBestMatchAsync(int companyId, int? departmentId, int? categoryId, int? subCategoryId, string? priority, string? ticketType)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@DepartmentId", departmentId, DbType.Int32);
        parameters.Add("@CategoryId", categoryId, DbType.Int32);
        parameters.Add("@SubCategoryId", subCategoryId, DbType.Int32);
        parameters.Add("@Priority", priority, DbType.String, size: 20);
        parameters.Add("@TicketType", ticketType, DbType.String, size: 30);

        return await connection.QuerySingleOrDefaultAsync<SlaPolicy>(
            "dbo.SlaPolicy_GetBestMatch",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    private sealed class SlaPolicyListingDbRow : SlaPolicy
    {
        public new int TotalCount { get; set; }
    }
}
