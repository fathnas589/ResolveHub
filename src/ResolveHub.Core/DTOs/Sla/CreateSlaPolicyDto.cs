namespace ResolveHub.Core.DTOs.Sla;

using System;
using System.ComponentModel.DataAnnotations;

public class CreateSlaPolicyDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? DepartmentGId { get; set; }
    public Guid? CategoryGId { get; set; }
    public Guid? SubCategoryGId { get; set; }

    [MaxLength(20)]
    public string? Priority { get; set; }

    [MaxLength(30)]
    public string? TicketType { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "FirstResponseMinutes must be greater than 0.")]
    public int FirstResponseMinutes { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "ResolutionMinutes must be greater than 0.")]
    public int ResolutionMinutes { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "EscalationBeforeMinutes must be non-negative.")]
    public int EscalationBeforeMinutes { get; set; } = 0;

    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
