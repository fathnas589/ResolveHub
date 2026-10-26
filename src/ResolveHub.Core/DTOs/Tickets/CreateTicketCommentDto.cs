namespace ResolveHub.Core.DTOs.Tickets;

using System.ComponentModel.DataAnnotations;

public class CreateTicketCommentDto
{
    [Required(ErrorMessage = "Comment text is required.")]
    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
    public string CommentText { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;
}
