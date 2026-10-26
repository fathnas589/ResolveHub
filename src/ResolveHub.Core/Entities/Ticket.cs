namespace ResolveHub.Core.Entities;

using System;

public class Ticket
{
    public int Id { get; set; }
    public Guid GId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public int CreatedByUserId { get; set; }
    public int DepartmentId { get; set; }
    public int CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public string TicketType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedToUserId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
    public int? LastUpdBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Joined presentation fields
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

    // Pagination helper
    public int TotalCount { get; set; }
}
