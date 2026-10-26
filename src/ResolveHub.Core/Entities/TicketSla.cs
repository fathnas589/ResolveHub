namespace ResolveHub.Core.Entities;

using System;

public class TicketSla
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public int TicketId { get; set; }
    public int SlaPolicyId { get; set; }
    public int FirstResponseMinutes { get; set; }
    public int ResolutionMinutes { get; set; }
    public DateTime FirstResponseDueAt { get; set; }
    public DateTime? FirstRespondedAt { get; set; }
    public bool FirstResponseBreached { get; set; }
    public DateTime ResolutionDueAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool ResolutionBreached { get; set; }
    public DateTime? EscalationDueAt { get; set; }
    public DateTime? EscalationTriggeredAt { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }

    // Joined presentation fields
    public string? TicketNumber { get; set; }
    public Guid? SlaPolicyGId { get; set; }
    public string? SlaPolicyName { get; set; }
}
