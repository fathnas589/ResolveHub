namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Tickets;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class TicketRepository : ITicketRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public TicketRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Ticket?> InsertAsync(
        int companyId,
        int createdByUserId,
        Guid departmentGId,
        Guid categoryGId,
        Guid? subCategoryGId,
        string ticketType,
        string subject,
        string description,
        string priority)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@CreatedByUserId", createdByUserId, DbType.Int32);
        parameters.Add("@DepartmentGId", departmentGId, DbType.Guid);
        parameters.Add("@CategoryGId", categoryGId, DbType.Guid);
        parameters.Add("@SubCategoryGId", subCategoryGId, DbType.Guid);
        parameters.Add("@TicketType", ticketType, DbType.String, size: 30);
        parameters.Add("@Subject", subject, DbType.String, size: 200);
        parameters.Add("@Description", description, DbType.String);
        parameters.Add("@Priority", priority, DbType.String, size: 20);

        return await connection.QuerySingleOrDefaultAsync<Ticket>(
            "dbo.Ticket_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Ticket?> GetByGIdAsync(
        int companyId,
        Guid gId,
        int requestingUserId,
        string requestingRole)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@GId", gId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RequestingUserId", requestingUserId, DbType.Int32);
        parameters.Add("@RequestingRole", requestingRole, DbType.String, size: 50);

        return await connection.QuerySingleOrDefaultAsync<Ticket>(
            "dbo.Ticket_Get",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PagedResult<Ticket>> GetListingAsync(
        int companyId,
        int requestingUserId,
        string requestingRole,
        TicketListRequestDto filter)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RequestingUserId", requestingUserId, DbType.Int32);
        parameters.Add("@RequestingRole", requestingRole, DbType.String, size: 50);
        parameters.Add("@PageNumber", filter.PageNumber, DbType.Int32);
        parameters.Add("@PageSize", filter.PageSize, DbType.Int32);
        parameters.Add("@TicketType", string.IsNullOrWhiteSpace(filter.TicketType) ? null : filter.TicketType.Trim(), DbType.String, size: 30);
        parameters.Add("@Status", string.IsNullOrWhiteSpace(filter.Status) ? null : filter.Status.Trim(), DbType.String, size: 30);
        parameters.Add("@Priority", string.IsNullOrWhiteSpace(filter.Priority) ? null : filter.Priority.Trim(), DbType.String, size: 20);
        parameters.Add("@DepartmentGId", filter.DepartmentGId, DbType.Guid);
        parameters.Add("@CategoryGId", filter.CategoryGId, DbType.Guid);
        parameters.Add("@SubCategoryGId", filter.SubCategoryGId, DbType.Guid);
        parameters.Add("@AssignedToUserGId", filter.AssignedToUserGId, DbType.Guid);
        parameters.Add("@CreatedByUserGId", filter.CreatedByUserGId, DbType.Guid);
        parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(filter.SearchTerm) ? null : filter.SearchTerm.Trim(), DbType.String, size: 100);
        parameters.Add("@CreatedFrom", filter.CreatedFrom, DbType.DateTime2);
        parameters.Add("@CreatedTo", filter.CreatedTo, DbType.DateTime2);

        var rows = (await connection.QueryAsync<Ticket>(
            "dbo.Ticket_Listing",
            parameters,
            commandType: CommandType.StoredProcedure)).ToList();

        int totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;

        return new PagedResult<Ticket>
        {
            Items = rows,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<Ticket?> UpdateAsync(
        int companyId,
        Guid ticketGId,
        Guid departmentGId,
        Guid categoryGId,
        Guid? subCategoryGId,
        string subject,
        string description,
        string priority,
        int actorUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketGId", ticketGId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@DepartmentGId", departmentGId, DbType.Guid);
        parameters.Add("@CategoryGId", categoryGId, DbType.Guid);
        parameters.Add("@SubCategoryGId", subCategoryGId, DbType.Guid);
        parameters.Add("@Subject", subject, DbType.String, size: 200);
        parameters.Add("@Description", description, DbType.String);
        parameters.Add("@Priority", priority, DbType.String, size: 20);
        parameters.Add("@ActorUserId", actorUserId, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<Ticket>(
            "dbo.Ticket_Update",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Ticket?> AssignAsync(
        int companyId,
        Guid ticketGId,
        Guid assignedToUserGId,
        int actorUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketGId", ticketGId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@AssignedToUserGId", assignedToUserGId, DbType.Guid);
        parameters.Add("@ActorUserId", actorUserId, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<Ticket>(
            "dbo.Ticket_Assign",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Ticket?> UpdateStatusAsync(
        int companyId,
        Guid ticketGId,
        string newStatus,
        string? comments,
        int actorUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketGId", ticketGId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@NewStatus", newStatus, DbType.String, size: 30);
        parameters.Add("@Comments", comments, DbType.String, size: 1000);
        parameters.Add("@ActorUserId", actorUserId, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<Ticket>(
            "dbo.Ticket_UpdateStatus",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeleteAsync(int companyId, Guid ticketGId, int actorUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketGId", ticketGId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@ActorUserId", actorUserId, DbType.Int32);

        return await connection.ExecuteScalarAsync<bool>(
            "dbo.Ticket_Delete",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<TicketComment?> InsertCommentAsync(
        int companyId,
        Guid ticketGId,
        string commentText,
        bool isInternal,
        int createdByUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketGId", ticketGId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@CommentText", commentText, DbType.String, size: 2000);
        parameters.Add("@IsInternal", isInternal, DbType.Boolean);
        parameters.Add("@CreatedByUserId", createdByUserId, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<TicketComment>(
            "dbo.TicketComment_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<TicketComment>> GetCommentsAsync(
        int companyId,
        Guid ticketGId,
        int requestingUserId,
        string requestingRole)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketGId", ticketGId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RequestingUserId", requestingUserId, DbType.Int32);
        parameters.Add("@RequestingRole", requestingRole, DbType.String, size: 50);

        return await connection.QueryAsync<TicketComment>(
            "dbo.TicketComment_Listing",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<TicketHistory>> GetHistoryAsync(
        int companyId,
        Guid ticketGId,
        int requestingUserId,
        string requestingRole)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketGId", ticketGId, DbType.Guid);
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RequestingUserId", requestingUserId, DbType.Int32);
        parameters.Add("@RequestingRole", requestingRole, DbType.String, size: 50);

        return await connection.QueryAsync<TicketHistory>(
            "dbo.TicketHistory_Listing",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
