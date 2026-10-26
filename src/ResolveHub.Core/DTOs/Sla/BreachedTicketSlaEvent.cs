namespace ResolveHub.Core.DTOs.Sla;

using System;

public class BreachedTicketSlaEvent
{
    public int TicketId { get; set; }
    public Guid TicketGId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string BreachType { get; set; } = string.Empty; // 'FIRST_RESPONSE' or 'RESOLUTION'
    public DateTime DueAt { get; set; }
    public DateTime BreachedAt { get; set; }
}
