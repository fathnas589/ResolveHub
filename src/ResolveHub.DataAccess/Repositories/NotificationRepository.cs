namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Notifications;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class NotificationRepository : INotificationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public NotificationRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Notification?> InsertAsync(NotificationEvent notificationEvent)
    {
        ArgumentNullException.ThrowIfNull(notificationEvent);

        using var connection = _connectionFactory.CreateConnection();

        string recipientIdsCsv = string.Join(",", notificationEvent.RecipientUserIds);

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", notificationEvent.CompanyId, DbType.Int32);
        parameters.Add("@EventType", notificationEvent.EventType, DbType.String);
        parameters.Add("@Title", notificationEvent.Title, DbType.String);
        parameters.Add("@Message", notificationEvent.Message, DbType.String);
        parameters.Add("@TicketId", notificationEvent.TicketId, DbType.Int32);
        parameters.Add("@ActorUserId", notificationEvent.ActorUserId, DbType.Int32);
        parameters.Add("@ActionUrl", notificationEvent.ActionUrl, DbType.String);
        parameters.Add("@MetadataJson", notificationEvent.MetadataJson, DbType.String);
        parameters.Add("@CorrelationKey", notificationEvent.CorrelationKey, DbType.String);
        parameters.Add("@CreatedBy", notificationEvent.ActorUserId, DbType.Int32);
        parameters.Add("@RecipientUserIdsCsv", recipientIdsCsv, DbType.String);
        parameters.Add("@QueueEmail", notificationEvent.QueueEmail, DbType.Boolean);

        return await connection.QuerySingleOrDefaultAsync<Notification>(
            "dbo.Notification_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<PagedResult<NotificationResponseDto>> GetListingAsync(int companyId, int recipientUserId, NotificationListRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RecipientUserId", recipientUserId, DbType.Int32);
        parameters.Add("@UnreadOnly", request.UnreadOnly, DbType.Boolean);
        parameters.Add("@EventType", request.EventType, DbType.String);
        parameters.Add("@PageNumber", request.PageNumber, DbType.Int32);
        parameters.Add("@PageSize", request.PageSize, DbType.Int32);

        using var multi = await connection.QueryMultipleAsync(
            "dbo.Notification_Listing",
            parameters,
            commandType: CommandType.StoredProcedure);

        int totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<NotificationResponseDto>()).ToList();

        return new PagedResult<NotificationResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(int companyId, int recipientUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RecipientUserId", recipientUserId, DbType.Int32);

        return await connection.ExecuteScalarAsync<int>(
            "dbo.Notification_GetUnreadCount",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> MarkReadAsync(int companyId, int recipientUserId, Guid notificationGId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RecipientUserId", recipientUserId, DbType.Int32);
        parameters.Add("@NotificationGId", notificationGId, DbType.Guid);

        int rowsAffected = await connection.ExecuteScalarAsync<int>(
            "dbo.Notification_MarkRead",
            parameters,
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }

    public async Task<int> MarkAllReadAsync(int companyId, int recipientUserId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@RecipientUserId", recipientUserId, DbType.Int32);

        return await connection.ExecuteScalarAsync<int>(
            "dbo.Notification_MarkAllRead",
            parameters,
            commandType: CommandType.StoredProcedure);
    }
}
