namespace ResolveHub.Core.DTOs.Notifications;

using System;

public class PendingDeliveryItem
{
    public int DeliveryId { get; set; }
    public Guid DeliveryGId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public int RecipientUserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public int NotificationId { get; set; }
    public Guid NotificationGId { get; set; }
    public int CompanyId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public Guid? TicketGId { get; set; }
    public string? TicketNumber { get; set; }
}
