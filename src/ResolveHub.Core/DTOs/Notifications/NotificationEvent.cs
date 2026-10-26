namespace ResolveHub.Core.DTOs.Notifications;

using System;
using System.Collections.Generic;

public class NotificationEvent
{
    public string EventType { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int? TicketId { get; set; }
    public Guid? TicketGId { get; set; }
    public string? TicketNumber { get; set; }
    public int? ActorUserId { get; set; }
    public IReadOnlyList<int> RecipientUserIds { get; set; } = Array.Empty<int>();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string? CorrelationKey { get; set; }
    public string? MetadataJson { get; set; }
    public bool QueueEmail { get; set; } = true;
}

public static class NotificationEventTypes
{
    public const string TicketCreated = "TICKET_CREATED";
    public const string TicketAssigned = "TICKET_ASSIGNED";
    public const string TicketReassigned = "TICKET_REASSIGNED";
    public const string TicketPublicCommentAdded = "TICKET_PUBLIC_COMMENT_ADDED";
    public const string TicketStatusChanged = "TICKET_STATUS_CHANGED";
    public const string TicketResolved = "TICKET_RESOLVED";
    public const string TicketReopened = "TICKET_REOPENED";
    public const string SlaWarning = "SLA_WARNING";
    public const string FirstResponseSlaBreached = "FIRST_RESPONSE_SLA_BREACHED";
    public const string ResolutionSlaBreached = "RESOLUTION_SLA_BREACHED";
    public const string UserAccountCreated = "USER_ACCOUNT_CREATED";
}
