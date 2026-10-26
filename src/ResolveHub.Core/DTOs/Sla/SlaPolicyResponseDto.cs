namespace ResolveHub.Core.DTOs.Sla;

using System;

public class SlaPolicyResponseDto
{
    public Guid GId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? DepartmentGId { get; set; }
    public string? DepartmentName { get; set; }

    public Guid? CategoryGId { get; set; }
    public string? CategoryName { get; set; }

    public Guid? SubCategoryGId { get; set; }
    public string? SubCategoryName { get; set; }

    public string? Priority { get; set; }
    public string? TicketType { get; set; }

    public int FirstResponseMinutes { get; set; }
    public int ResolutionMinutes { get; set; }
    public int EscalationBeforeMinutes { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    public DateTime Created { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? LastUpd { get; set; }
    public string? LastUpdByName { get; set; }
}
