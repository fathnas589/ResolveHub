namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.SubCategories;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class SubCategoryRepository : ISubCategoryRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public SubCategoryRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<SubCategory?> InsertAsync(SubCategory subCategory)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", subCategory.CompanyId, DbType.Int32);
        parameters.Add("@CategoryGId", subCategory.CategoryGId, DbType.Guid);
        parameters.Add("@SubCategoryName", subCategory.SubCategoryName, DbType.String, size: 100);
        parameters.Add("@Description", subCategory.Description, DbType.String, size: 250);
        parameters.Add("@IsActive", subCategory.IsActive, DbType.Boolean);
        parameters.Add("@CreatedBy", subCategory.CreatedBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<SubCategory>(
            "dbo.SubCategory_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<SubCategory?> UpdateAsync(SubCategory subCategory)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", subCategory.CompanyId, DbType.Int32);
        parameters.Add("@GId", subCategory.GId, DbType.Guid);
        parameters.Add("@CategoryGId", subCategory.CategoryGId, DbType.Guid);
        parameters.Add("@SubCategoryName", subCategory.SubCategoryName, DbType.String, size: 100);
        parameters.Add("@Description", subCategory.Description, DbType.String, size: 250);
        parameters.Add("@IsActive", subCategory.IsActive, DbType.Boolean);
        parameters.Add("@LastUpdBy", subCategory.LastUpdBy, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<SubCategory>(
            "dbo.SubCategory_Update",
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
            "dbo.SubCategory_Delete",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<SubCategory?> GetByGIdAsync(int companyId, Guid gId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@GId", gId, DbType.Guid);

        return await connection.QuerySingleOrDefaultAsync<SubCategory>(
            "dbo.SubCategory_Get",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> CheckDuplicateNameAsync(int companyId, Guid categoryGId, string subCategoryName, Guid? excludeGId = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@CategoryGId", categoryGId, DbType.Guid);
        parameters.Add("@SubCategoryName", subCategoryName, DbType.String, size: 100);
        parameters.Add("@ExcludeGId", excludeGId, DbType.Guid);

        return await connection.ExecuteScalarAsync<bool>(
            "dbo.SubCategory_DuplicateCheck",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PagedResult<SubCategory>> GetListingAsync(int companyId, SubCategoryListRequestDto request)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@CategoryGId", request.CategoryGId, DbType.Guid);
        parameters.Add("@DepartmentGId", request.DepartmentGId, DbType.Guid);
        parameters.Add("@SearchTerm", request.SearchTerm, DbType.String, size: 100);
        parameters.Add("@IsActive", request.IsActive, DbType.Boolean);
        parameters.Add("@PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("@PageSize", request.PageSize, DbType.Int32);

        var rows = (await connection.QueryAsync<SubCategoryListingDbRow>(
            "dbo.SubCategory_Listing",
            parameters,
            commandType: CommandType.StoredProcedure)).ToList();

        int totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new PagedResult<SubCategory>
        {
            Items = rows.Cast<SubCategory>().ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    /// <summary>
    /// Internal row type used exclusively to capture the TotalCount window function column
    /// returned alongside subcategory listing records.
    /// </summary>
    private sealed class SubCategoryListingDbRow : SubCategory
    {
        public int TotalCount { get; set; }
    }
}
