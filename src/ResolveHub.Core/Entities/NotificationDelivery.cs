namespace ResolveHub.Core.Entities;

using System;

public class NotificationDelivery
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public int NotificationRecipientId { get; set; }
    public string Channel { get; set; } = "EMAIL";
    public string Status { get; set; } = "PENDING";
    public int AttemptCount { get; set; }
    public DateTime NextAttemptAt { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? LastError { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
}
