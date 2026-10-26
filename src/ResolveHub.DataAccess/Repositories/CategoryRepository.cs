namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Categories;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class CategoryRepository : ICategoryRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public CategoryRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Category?> InsertAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", category.CompanyId, DbType.Int32);
        parameters.Add("@DepartmentGId", category.DepartmentGId, DbType.Guid);
        parameters.Add("@CategoryName", category.CategoryName, DbType.String, size: 100);
        parameters.Add("@Description", category.Description, DbType.String, size: 500);
        parameters.Add("@IsActive", category.IsActive, DbType.Boolean);
        parameters.Add("@CreatedBy", category.CreatedBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<Category>(
            "dbo.Category_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", category.CompanyId, DbType.Int32);
        parameters.Add("@GId", category.GId, DbType.Guid);
        parameters.Add("@DepartmentGId", category.DepartmentGId, DbType.Guid);
        parameters.Add("@CategoryName", category.CategoryName, DbType.String, size: 100);
        parameters.Add("@Description", category.Description, DbType.String, size: 500);
        parameters.Add("@IsActive", category.IsActive, DbType.Boolean);
        parameters.Add("@LastUpdBy", category.LastUpdBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<Category>(
            "dbo.Category_Update",
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
            "dbo.Category_Delete",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Category?> GetByGIdAsync(int companyId, Guid gId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<Category>(
            "dbo.Category_Get",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> CheckDuplicateNameAsync(int companyId, Guid departmentGId, string categoryName, Guid? excludeGId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@DepartmentGId", departmentGId, DbType.Guid);
        parameters.Add("@CategoryName", categoryName, DbType.String, size: 100);
        parameters.Add("@ExcludeGId", excludeGId, DbType.Guid);

        return await connection.ExecuteScalarAsync<bool>(
            "dbo.Category_DuplicateCheck",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PagedResult<Category>> GetListingAsync(int companyId, CategoryListRequestDto request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@DepartmentGId", request.DepartmentGId, DbType.Guid);
        parameters.Add("@SearchTerm", request.SearchTerm, DbType.String, size: 100);
        parameters.Add("@IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("@PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("@PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<CategoryListingDbRow>(
            "dbo.Category_Listing",
            parameters,
            commandType: CommandType.StoredProcedure)).ToList();

        int totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new PagedResult<Category>
        {
            Items = rows.Cast<Category>().ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// Internal row type used exclusively to capture the TotalCount window function column
    /// returned alongside category listing records.
    /// </summary>
    private sealed class CategoryListingDbRow : Category
    {
        public int TotalCount { get; set; }
    }
}
