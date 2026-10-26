namespace ResolveHub.Core.DTOs.Sla;

using System;

public class TicketSlaResponseDto
{
    public Guid GId { get; set; }
    public Guid? SlaPolicyGId { get; set; }
    public string? SlaPolicyName { get; set; }

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

    public string SlaHealth { get; set; } = "ON_TRACK";
    public long? FirstResponseRemainingSeconds { get; set; }
    public long? ResolutionRemainingSeconds { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
}
