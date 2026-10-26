namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Notifications;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class NotificationDeliveryRepository : INotificationDeliveryRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public NotificationDeliveryRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PendingDeliveryItem>> ClaimPendingDeliveriesAsync(int batchSize, DateTime? currentUtc = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@BatchSize", batchSize, DbType.Int32);
        parameters.Add("@CurrentUtc", currentUtc ?? DateTime.UtcNow, DbType.DateTime2);

        var items = await connection.QueryAsync<PendingDeliveryItem>(
            "dbo.NotificationDelivery_ClaimPending",
            parameters,
            commandType: CommandType.StoredProcedure);

        return items.ToList();
    }

    public async Task<bool> UpdateDeliveryStatusAsync(
        int deliveryId,
        bool isSuccess,
        string? lastError = null,
        DateTime? nextAttemptAt = null,
        int maxAttempts = 5,
        DateTime? currentUtc = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@DeliveryId", deliveryId, DbType.Int32);
        parameters.Add("@IsSuccess", isSuccess, DbType.Boolean);
        parameters.Add("@LastError", lastError, DbType.String);
        parameters.Add("@NextAttemptAt", nextAttemptAt, DbType.DateTime2);
        parameters.Add("@MaxAttempts", maxAttempts, DbType.Int32);
        parameters.Add("@CurrentUtc", currentUtc ?? DateTime.UtcNow, DbType.DateTime2);

        int rowsAffected = await connection.ExecuteScalarAsync<int>(
            "dbo.NotificationDelivery_UpdateStatus",
            parameters,
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }
}
