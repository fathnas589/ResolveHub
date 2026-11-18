namespace ResolveHub.Core.Entities;

using System;

public class TicketComment
{
    public long Id { get; set; }
    public Guid GId { get; set; }
    public int TicketId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public int CreatedByUserId { get; set; }
    public bool IsInternal { get; set; }
    public bool IsDeleted { get; set; }

    // Joined presentation fields
    public Guid AuthorUserGId { get; set; }
    public string AuthorUserName { get; set; } = string.Empty;
    public string AuthorFullName { get; set; } = string.Empty;
    public string AuthorRoleCode { get; set; } = string.Empty;
}
