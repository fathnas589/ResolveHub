namespace ResolveHub.Core.DTOs.Tickets;

using System;

public class TicketListRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? TicketType { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public Guid? DepartmentGId { get; set; }
    public Guid? CategoryGId { get; set; }
    public Guid? SubCategoryGId { get; set; }
    public Guid? AssignedToUserGId { get; set; }
    public Guid? CreatedByUserGId { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
