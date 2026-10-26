namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Notifications;
using ResolveHub.Core.Entities;

public interface INotificationRepository
{
    Task<Notification?> InsertAsync(NotificationEvent notificationEvent);
    Task<PagedResult<NotificationResponseDto>> GetListingAsync(int companyId, int recipientUserId, NotificationListRequestDto request);
    Task<int> GetUnreadCountAsync(int companyId, int recipientUserId);
    Task<bool> MarkReadAsync(int companyId, int recipientUserId, Guid notificationGId);
    Task<int> MarkAllReadAsync(int companyId, int recipientUserId);
}
