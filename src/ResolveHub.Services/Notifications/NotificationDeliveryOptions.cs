namespace ResolveHub.Services.Notifications;

public class NotificationDeliveryOptions
{
    public bool Enabled { get; set; } = true;
    public int PollIntervalSeconds { get; set; } = 15;
    public int BatchSize { get; set; } = 25;
    public int MaxAttempts { get; set; } = 5;
}
