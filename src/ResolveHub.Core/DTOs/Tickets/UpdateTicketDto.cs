namespace ResolveHub.Core.DTOs.Tickets;

using System;
using System.ComponentModel.DataAnnotations;

public class UpdateTicketDto
{
    [Required(ErrorMessage = "Department is required.")]
    public Guid DepartmentGId { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public Guid CategoryGId { get; set; }

    public Guid? SubCategoryGId { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [MaxLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Priority is required.")]
    [MaxLength(20, ErrorMessage = "Priority cannot exceed 20 characters.")]
    public string Priority { get; set; } = string.Empty;
}
