namespace ResolveHub.Core.DTOs.Sla;

using System;

public class WarningTicketSlaEvent
{
    public int TicketId { get; set; }
    public Guid TicketGId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int? AssignedToUserId { get; set; }
    public DateTime EscalationDueAt { get; set; }
    public DateTime TriggeredAt { get; set; }
}
