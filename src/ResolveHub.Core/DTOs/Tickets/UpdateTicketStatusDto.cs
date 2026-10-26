namespace ResolveHub.Core.DTOs.Tickets;

using System.ComponentModel.DataAnnotations;

public class UpdateTicketStatusDto
{
    [Required(ErrorMessage = "Status is required.")]
    [MaxLength(30, ErrorMessage = "Status cannot exceed 30 characters.")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters.")]
    public string? Comments { get; set; }
}
