namespace ResolveHub.Core.Entities;

using System;

public class SlaPolicy
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DepartmentId { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public string? Priority { get; set; }
    public string? TicketType { get; set; }
    public int FirstResponseMinutes { get; set; }
    public int ResolutionMinutes { get; set; }
    public int EscalationBeforeMinutes { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime Created { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? LastUpd { get; set; }
    public int? LastUpdBy { get; set; }

    // Joined metadata
    public Guid? DepartmentGId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? CategoryGId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? SubCategoryGId { get; set; }
    public string? SubCategoryName { get; set; }
    public string? CreatedByName { get; set; }
    public string? LastUpdByName { get; set; }

    // Listing helper
    public int TotalCount { get; set; }

    // Specificity score (used in matching)
    public int SpecificityScore { get; set; }
}
