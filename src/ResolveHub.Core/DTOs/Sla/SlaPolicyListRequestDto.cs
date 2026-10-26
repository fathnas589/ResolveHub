namespace ResolveHub.Core.DTOs.Sla;

using System;

public class SlaPolicyListRequestDto
{
    public string? SearchTerm { get; set; }
    public Guid? DepartmentGId { get; set; }
    public Guid? CategoryGId { get; set; }
    public string? Priority { get; set; }
    public string? TicketType { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
