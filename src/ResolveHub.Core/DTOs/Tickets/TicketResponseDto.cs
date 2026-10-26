namespace ResolveHub.Core.DTOs.Tickets;

using System;

using ResolveHub.Core.DTOs.Sla;

public class TicketResponseDto
{
    public Guid GId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public Guid DepartmentGId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public Guid CategoryGId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public Guid? SubCategoryGId { get; set; }
    public string? SubCategoryName { get; set; }

    public Guid CreatedByUserGId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public string CreatedByFullName { get; set; } = string.Empty;

    public Guid? AssignedToUserGId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? AssignedToFullName { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
    public DateTime? ClosedAt { get; set; }

    public TicketSlaSummaryDto? Sla { get; set; }
}
