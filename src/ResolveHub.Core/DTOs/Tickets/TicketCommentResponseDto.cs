namespace ResolveHub.Core.DTOs.Tickets;

using System;

public class TicketCommentResponseDto
{
    public Guid GId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime Created { get; set; }

    public Guid AuthorUserGId { get; set; }
    public string AuthorUserName { get; set; } = string.Empty;
    public string AuthorFullName { get; set; } = string.Empty;
    public string AuthorRoleCode { get; set; } = string.Empty;
}
