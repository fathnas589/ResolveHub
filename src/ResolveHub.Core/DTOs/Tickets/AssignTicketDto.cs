namespace ResolveHub.Core.DTOs.Tickets;

using System;
using System.ComponentModel.DataAnnotations;

public class AssignTicketDto
{
    [Required(ErrorMessage = "Assigned user GId is required.")]
    public Guid AssignedToUserGId { get; set; }
}
