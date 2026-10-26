namespace ResolveHub.Core.DTOs.Departments;

using System;

public class DepartmentResponseDto
{
    public Guid GId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastUpd { get; set; }
}
