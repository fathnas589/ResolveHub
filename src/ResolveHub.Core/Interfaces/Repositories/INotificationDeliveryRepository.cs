namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Notifications;

public interface INotificationDeliveryRepository
{
    Task<IReadOnlyList<PendingDeliveryItem>> ClaimPendingDeliveriesAsync(int batchSize, DateTime? currentUtc = null);
    Task<bool> UpdateDeliveryStatusAsync(int deliveryId, bool isSuccess, string? lastError = null, DateTime? nextAttemptAt = null, int maxAttempts = 5, DateTime? currentUtc = null);
}
