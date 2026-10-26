namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Tickets;
using ResolveHub.Core.Entities;

public interface ITicketRepository
{
    Task<Ticket?> InsertAsync(
        int companyId,
        int createdByUserId,
        Guid departmentGId,
        Guid categoryGId,
        Guid? subCategoryGId,
        string ticketType,
        string subject,
        string description,
        string priority);

    Task<Ticket?> GetByGIdAsync(
        int companyId,
        Guid gId,
        int requestingUserId,
        string requestingRole);

    Task<PagedResult<Ticket>> GetListingAsync(
        int companyId,
        int requestingUserId,
        string requestingRole,
        TicketListRequestDto filter);

    Task<Ticket?> UpdateAsync(
        int companyId,
        Guid ticketGId,
        Guid departmentGId,
        Guid categoryGId,
        Guid? subCategoryGId,
        string subject,
        string description,
        string priority,
        int actorUserId);

    Task<Ticket?> AssignAsync(
        int companyId,
        Guid ticketGId,
        Guid assignedToUserGId,
        int actorUserId);

    Task<Ticket?> UpdateStatusAsync(
        int companyId,
        Guid ticketGId,
        string newStatus,
        string? comments,
        int actorUserId);

    Task<bool> DeleteAsync(
        int companyId,
        Guid ticketGId,
        int actorUserId);

    Task<TicketComment?> InsertCommentAsync(
        int companyId,
        Guid ticketGId,
        string commentText,
        bool isInternal,
        int createdByUserId);

    Task<IEnumerable<TicketComment>> GetCommentsAsync(
        int companyId,
        Guid ticketGId,
        int requestingUserId,
        string requestingRole);

    Task<IEnumerable<TicketHistory>> GetHistoryAsync(
        int companyId,
        Guid ticketGId,
        int requestingUserId,
        string requestingRole);
}
