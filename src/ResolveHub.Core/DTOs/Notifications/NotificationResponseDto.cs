namespace ResolveHub.Core.DTOs.Notifications;

using System;

public class NotificationResponseDto
{
    public Guid GId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime Created { get; set; }
    public Guid? TicketGId { get; set; }
    public string? TicketNumber { get; set; }
}
