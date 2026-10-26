namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Departments;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DepartmentRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Department?> InsertAsync(Department department)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", department.CompanyId, DbType.Int32);
        parameters.Add("@DepartmentName", department.DepartmentName, DbType.String, size: 100);
        parameters.Add("@Description", department.Description, DbType.String, size: 500);
        parameters.Add("@IsActive", department.IsActive, DbType.Boolean);
        parameters.Add("@CreatedBy", department.CreatedBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<Department>(
            "dbo.Department_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Department?> UpdateAsync(Department department)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", department.CompanyId, DbType.Int32);
        parameters.Add("@GId", department.GId, DbType.Guid);
        parameters.Add("@DepartmentName", department.DepartmentName, DbType.String, size: 100);
        parameters.Add("@Description", department.Description, DbType.String, size: 500);
        parameters.Add("@IsActive", department.IsActive, DbType.Boolean);
        parameters.Add("@LastUpdBy", department.LastUpdBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<Department>(
            "dbo.Department_Update",
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
            "dbo.Department_Delete",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Department?> GetByGIdAsync(int companyId, Guid gId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<Department>(
            "dbo.Department_Get",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> CheckDuplicateNameAsync(int companyId, string departmentName, Guid? excludeGId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@DepartmentName", departmentName, DbType.String, size: 100);
        parameters.Add("@ExcludeGId", excludeGId, DbType.Guid);

        return await connection.ExecuteScalarAsync<bool>(
            "dbo.Department_DuplicateCheck",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PagedResult<Department>> GetListingAsync(int companyId, DepartmentListRequestDto request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@SearchTerm", request.SearchTerm, DbType.String, size: 100);
        parameters.Add("@IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("@PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("@PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<DepartmentListingDbRow>(
            "dbo.Department_Listing",
            parameters,
            commandType: CommandType.StoredProcedure)).ToList();

        int totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new PagedResult<Department>
        {
            Items = rows.Cast<Department>().ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// Internal row type used exclusively to capture the TotalCount window function column
    /// returned alongside department listing records.
    /// </summary>
    private sealed class DepartmentListingDbRow : Department
    {
        public int TotalCount { get; set; }
    }
}
