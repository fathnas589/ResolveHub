namespace ResolveHub.Services.Notifications;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Notifications;
using ResolveHub.Core.Entities;

public interface INotificationService
{
    Task<Notification?> PublishAsync(NotificationEvent notificationEvent);
    Task<PagedResult<NotificationResponseDto>> GetListingAsync(int companyId, int recipientUserId, NotificationListRequestDto request);
    Task<int> GetUnreadCountAsync(int companyId, int recipientUserId);
    Task<bool> MarkReadAsync(int companyId, int recipientUserId, Guid notificationGId);
    Task<int> MarkAllReadAsync(int companyId, int recipientUserId);

    // Lifecycle Helpers
    Task PublishTicketCreatedAsync(int companyId, int ticketId, Guid ticketGId, string ticketNumber, string subject, int createdByUserId, string creatorName, int? assignedToUserId);
    Task PublishTicketAssignedAsync(int companyId, int ticketId, Guid ticketGId, string ticketNumber, string subject, int assignedToUserId, int? actorUserId, string? actorName, bool isReassignment = false);
    Task PublishTicketCommentAddedAsync(int companyId, int ticketId, Guid ticketGId, string ticketNumber, string subject, int commentAuthorUserId, string commentAuthorName, bool isAuthorStaff, int ticketRequesterUserId, int? assignedToUserId);
    Task PublishTicketStatusChangedAsync(int companyId, int ticketId, Guid ticketGId, string ticketNumber, string subject, string oldStatus, string newStatus, int actorUserId, string actorName, int ticketRequesterUserId, int? assignedToUserId);
    Task PublishSlaWarningAsync(int companyId, int ticketId, Guid ticketGId, string ticketNumber, string subject, int? assignedToUserId, DateTime? dueAtUtc = null);
    Task PublishSlaBreachAsync(int companyId, int ticketId, Guid ticketGId, string ticketNumber, string subject, string breachType, int? assignedToUserId, DateTime? dueAtUtc = null);
}
