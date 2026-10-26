namespace ResolveHub.Core.DTOs.Sla;

using System;

public class TicketSlaSummaryDto
{
    public string? SlaPolicyName { get; set; }
    public string SlaHealth { get; set; } = "ON_TRACK";
    public DateTime? FirstResponseDueAt { get; set; }
    public DateTime? ResolutionDueAt { get; set; }
    public bool FirstResponseBreached { get; set; }
    public bool ResolutionBreached { get; set; }
    public long? FirstResponseRemainingSeconds { get; set; }
    public long? ResolutionRemainingSeconds { get; set; }
}
