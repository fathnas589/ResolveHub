namespace ResolveHub.Core.DTOs.Notifications;

public class NotificationListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool UnreadOnly { get; set; } = false;
    public string? EventType { get; set; }
}

public class UnreadNotificationCountDto
{
    public int UnreadCount { get; set; }
}
