namespace ResolveHub.Core.Entities;

using System;

public class Notification
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public int CompanyId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? TicketId { get; set; }
    public string? TicketNumber { get; set; }
    public Guid? TicketGId { get; set; }
    public int? ActorUserId { get; set; }
    public string? ActorFullName { get; set; }
    public string? ActionUrl { get; set; }
    public string? MetadataJson { get; set; }
    public string? CorrelationKey { get; set; }
    public DateTime Created { get; set; }
    public int? CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
