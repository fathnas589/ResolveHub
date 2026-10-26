namespace ResolveHub.Core.DTOs.Tickets;

using System;

public class TicketHistoryResponseDto
{
    public Guid GId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Comments { get; set; }
    public DateTime Created { get; set; }

    public Guid ActorUserGId { get; set; }
    public string ActorUserName { get; set; } = string.Empty;
    public string ActorFullName { get; set; } = string.Empty;
}
