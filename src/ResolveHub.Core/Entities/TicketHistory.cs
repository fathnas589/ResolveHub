namespace ResolveHub.Core.Entities;

using System;

public class TicketHistory
{
    public long Id { get; set; }
    public Guid GId { get; set; }
    public int TicketId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Comments { get; set; }
    public DateTime Created { get; set; }
    public int CreatedByUserId { get; set; }

    // Joined presentation fields
    public Guid ActorUserGId { get; set; }
    public string ActorUserName { get; set; } = string.Empty;
    public string ActorFullName { get; set; } = string.Empty;
}
