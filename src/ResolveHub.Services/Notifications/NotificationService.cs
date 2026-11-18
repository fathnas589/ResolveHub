namespace ResolveHub.Services.Notifications;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Notifications;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Notification?> PublishAsync(NotificationEvent notificationEvent)
    {
        ArgumentNullException.ThrowIfNull(notificationEvent);

        if (notificationEvent.RecipientUserIds == null || notificationEvent.RecipientUserIds.Count == 0)
        {
            _logger.LogDebug("Skipping notification publication for event {EventType}: no recipients provided.", notificationEvent.EventType);
            return null;
        }

        try
        {
            var notification = await _notificationRepository.InsertAsync(notificationEvent);
            if (notification != null)
            {
                _logger.LogInformation("Notification {NotificationGId} published for event {EventType} (Recipients: {RecipientCount}).",
                    notification.GId, notificationEvent.EventType, notificationEvent.RecipientUserIds.Count);
            }
            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist notification for event {EventType} on Ticket {TicketId}.",
                notificationEvent.EventType, notificationEvent.TicketId);
            return null;
        }
    }

    public async Task<PagedResult<NotificationResponseDto>> GetListingAsync(int companyId, int recipientUserId, NotificationListRequestDto request)
    {
        return await _notificationRepository.GetListingAsync(companyId, recipientUserId, request);
    }

    public async Task<int> GetUnreadCountAsync(int companyId, int recipientUserId)
    {
        return await _notificationRepository.GetUnreadCountAsync(companyId, recipientUserId);
    }

    public async Task<bool> MarkReadAsync(int companyId, int recipientUserId, Guid notificationGId)
    {
        return await _notificationRepository.MarkReadAsync(companyId, recipientUserId, notificationGId);
    }

    public async Task<int> MarkAllReadAsync(int companyId, int recipientUserId)
    {
        return await _notificationRepository.MarkAllReadAsync(companyId, recipientUserId);
    }

    public async Task PublishTicketCreatedAsync(
        int companyId,
        int ticketId,
        Guid ticketGId,
        string ticketNumber,
        string subject,
        int createdByUserId,
        string creatorName,
        int? assignedToUserId)
    {
        var recipients = new List<int>();
        if (assignedToUserId.HasValue && assignedToUserId.Value > 0 && assignedToUserId.Value != createdByUserId)
        {
            recipients.Add(assignedToUserId.Value);
        }
        else
        {
            var adminIds = await GetCompanyAdminIdsAsync(companyId, createdByUserId);
            recipients.AddRange(adminIds);
        }

        if (recipients.Count == 0) return;

        var copy = NotificationMessageFactory.CreateTicketEventCopy(
            NotificationEventTypes.TicketCreated,
            ticketNumber,
            subject,
            ticketGId,
            actorName: creatorName);

        await PublishAsync(new NotificationEvent
        {
            EventType = NotificationEventTypes.TicketCreated,
            CompanyId = companyId,
            TicketId = ticketId,
            TicketGId = ticketGId,
            TicketNumber = ticketNumber,
            ActorUserId = createdByUserId,
            RecipientUserIds = recipients,
            Title = copy.Title,
            Message = copy.Message,
            ActionUrl = copy.ActionUrl,
            CorrelationKey = $"TICKET_CREATED:{ticketId}"
        });
    }

    public async Task PublishTicketAssignedAsync(
        int companyId,
        int ticketId,
        Guid ticketGId,
        string ticketNumber,
        string subject,
        int assignedToUserId,
        int? actorUserId,
        string? actorName,
        bool isReassignment = false)
    {
        if (actorUserId.HasValue && actorUserId.Value == assignedToUserId)
        {
            // Do not notify if self-assigned
            return;
        }

        string eventType = isReassignment ? NotificationEventTypes.TicketReassigned : NotificationEventTypes.TicketAssigned;
        var copy = NotificationMessageFactory.CreateTicketEventCopy(
            eventType,
            ticketNumber,
            subject,
            ticketGId,
            actorName: actorName);

        await PublishAsync(new NotificationEvent
        {
            EventType = eventType,
            CompanyId = companyId,
            TicketId = ticketId,
            TicketGId = ticketGId,
            TicketNumber = ticketNumber,
            ActorUserId = actorUserId,
            RecipientUserIds = new[] { assignedToUserId },
            Title = copy.Title,
            Message = copy.Message,
            ActionUrl = copy.ActionUrl,
            CorrelationKey = $"{eventType}:{ticketId}:{assignedToUserId}:{Guid.NewGuid().ToString("N")[..8]}"
        });
    }

    public async Task PublishTicketCommentAddedAsync(
        int companyId,
        int ticketId,
        Guid ticketGId,
        string ticketNumber,
        string subject,
        int commentAuthorUserId,
        string commentAuthorName,
        bool isAuthorStaff,
        int ticketRequesterUserId,
        int? assignedToUserId)
    {
        var recipients = new List<int>();

        if (isAuthorStaff)
        {
            // Staff posted a public comment -> notify requester if not author
            if (commentAuthorUserId != ticketRequesterUserId)
            {
                recipients.Add(ticketRequesterUserId);
            }
        }
        else
        {
            // End user posted -> notify assigned agent
            if (assignedToUserId.HasValue && assignedToUserId.Value != commentAuthorUserId)
            {
                recipients.Add(assignedToUserId.Value);
            }
        }

        if (recipients.Count == 0) return;

        var copy = NotificationMessageFactory.CreateTicketEventCopy(
            NotificationEventTypes.TicketPublicCommentAdded,
            ticketNumber,
            subject,
            ticketGId,
            actorName: commentAuthorName);

        await PublishAsync(new NotificationEvent
        {
            EventType = NotificationEventTypes.TicketPublicCommentAdded,
            CompanyId = companyId,
            TicketId = ticketId,
            TicketGId = ticketGId,
            TicketNumber = ticketNumber,
            ActorUserId = commentAuthorUserId,
            RecipientUserIds = recipients,
            Title = copy.Title,
            Message = copy.Message,
            ActionUrl = copy.ActionUrl
        });
    }

    public async Task PublishTicketStatusChangedAsync(
        int companyId,
        int ticketId,
        Guid ticketGId,
        string ticketNumber,
        string subject,
        string oldStatus,
        string newStatus,
        int actorUserId,
        string actorName,
        int ticketRequesterUserId,
        int? assignedToUserId)
    {
        if (string.Equals(oldStatus, newStatus, StringComparison.OrdinalIgnoreCase))
        {
            // No-op
            return;
        }

        var recipients = new HashSet<int>();
        if (ticketRequesterUserId != actorUserId)
        {
            recipients.Add(ticketRequesterUserId);
        }
        if (assignedToUserId.HasValue && assignedToUserId.Value != actorUserId)
        {
            recipients.Add(assignedToUserId.Value);
        }

        if (recipients.Count == 0) return;

        string eventType = (oldStatus, newStatus) switch
        {
            (_, "RESOLVED") => NotificationEventTypes.TicketResolved,
            ("RESOLVED", "IN_PROGRESS") => NotificationEventTypes.TicketReopened,
            (_, "REOPENED") => NotificationEventTypes.TicketReopened,
            _ => NotificationEventTypes.TicketStatusChanged
        };

        var copy = NotificationMessageFactory.CreateTicketEventCopy(
            eventType,
            ticketNumber,
            subject,
            ticketGId,
            actorName: actorName,
            oldStatus: oldStatus,
            newStatus: newStatus);

        await PublishAsync(new NotificationEvent
        {
            EventType = eventType,
            CompanyId = companyId,
            TicketId = ticketId,
            TicketGId = ticketGId,
            TicketNumber = ticketNumber,
            ActorUserId = actorUserId,
            RecipientUserIds = recipients.ToList(),
            Title = copy.Title,
            Message = copy.Message,
            ActionUrl = copy.ActionUrl,
            CorrelationKey = $"{eventType}:{ticketId}:{newStatus}:{Guid.NewGuid().ToString("N")[..8]}"
        });
    }

    public async Task PublishSlaWarningAsync(
        int companyId,
        int ticketId,
        Guid ticketGId,
        string ticketNumber,
        string subject,
        int? assignedToUserId,
        DateTime? dueAtUtc = null)
    {
        var recipients = new List<int>();
        if (assignedToUserId.HasValue && assignedToUserId.Value > 0)
        {
            recipients.Add(assignedToUserId.Value);
        }

        var adminIds = await GetCompanyAdminIdsAsync(companyId);
        foreach (var adminId in adminIds)
        {
            if (!recipients.Contains(adminId))
            {
                recipients.Add(adminId);
            }
        }

        if (recipients.Count == 0) return;

        var copy = NotificationMessageFactory.CreateTicketEventCopy(
            NotificationEventTypes.SlaWarning,
            ticketNumber,
            subject,
            ticketGId);

        await PublishAsync(new NotificationEvent
        {
            EventType = NotificationEventTypes.SlaWarning,
            CompanyId = companyId,
            TicketId = ticketId,
            TicketGId = ticketGId,
            TicketNumber = ticketNumber,
            RecipientUserIds = recipients,
            Title = copy.Title,
            Message = copy.Message,
            ActionUrl = copy.ActionUrl,
            CorrelationKey = dueAtUtc.HasValue ? $"SLA_WARNING:{ticketId}:{dueAtUtc.Value:yyyyMMddHHmmss}" : $"SLA_WARNING:{ticketId}"
        });
    }

    public async Task PublishSlaBreachAsync(
        int companyId,
        int ticketId,
        Guid ticketGId,
        string ticketNumber,
        string subject,
        string breachType,
        int? assignedToUserId,
        DateTime? dueAtUtc = null)
    {
        var recipients = new List<int>();
        if (assignedToUserId.HasValue && assignedToUserId.Value > 0)
        {
            recipients.Add(assignedToUserId.Value);
        }

        var adminIds = await GetCompanyAdminIdsAsync(companyId);
        foreach (var adminId in adminIds)
        {
            if (!recipients.Contains(adminId))
            {
                recipients.Add(adminId);
            }
        }

        if (recipients.Count == 0) return;

        string eventType = string.Equals(breachType, "FIRST_RESPONSE", StringComparison.OrdinalIgnoreCase)
            ? NotificationEventTypes.FirstResponseSlaBreached
            : NotificationEventTypes.ResolutionSlaBreached;

        var copy = NotificationMessageFactory.CreateTicketEventCopy(
            eventType,
            ticketNumber,
            subject,
            ticketGId);

        await PublishAsync(new NotificationEvent
        {
            EventType = eventType,
            CompanyId = companyId,
            TicketId = ticketId,
            TicketGId = ticketGId,
            TicketNumber = ticketNumber,
            RecipientUserIds = recipients,
            Title = copy.Title,
            Message = copy.Message,
            ActionUrl = copy.ActionUrl,
            CorrelationKey = dueAtUtc.HasValue ? $"{eventType}:{ticketId}:{dueAtUtc.Value:yyyyMMddHHmmss}" : $"{eventType}:{ticketId}"
        });
    }

    private async Task<List<int>> GetCompanyAdminIdsAsync(int companyId, int? excludeUserId = null)
    {
        var adminIds = new List<int>();
        try
        {
            var admins = await _userRepository.GetListingAsync(companyId, new ResolveHub.Core.DTOs.Users.UserListRequestDto
            {
                RoleCode = "ADMIN",
                PageSize = 100,
                IsActive = true
            });

            if (admins?.Items != null)
            {
                foreach (var admin in admins.Items)
                {
                    if (admin.Id > 0 && (!excludeUserId.HasValue || admin.Id != excludeUserId.Value))
                    {
                        adminIds.Add(admin.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve company admins for CompanyId {CompanyId}", companyId);
        }

        return adminIds;
    }
}
