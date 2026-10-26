namespace ResolveHub.Core.Entities;

using System;

public class NotificationRecipient
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public int NotificationId { get; set; }
    public int RecipientUserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
}
