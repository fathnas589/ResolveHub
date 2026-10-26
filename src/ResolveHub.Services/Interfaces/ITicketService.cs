namespace ResolveHub.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Common;
using ResolveHub.Core.DTOs.Tickets;

public interface ITicketService
{
    Task<TicketResponseDto> CreateTicketAsync(CreateTicketDto request);
    Task<TicketResponseDto> GetTicketByGIdAsync(Guid gId);
    Task<PagedResult<TicketResponseDto>> GetTicketsListingAsync(TicketListRequestDto request);
    Task<TicketResponseDto> UpdateTicketAsync(Guid gId, UpdateTicketDto request);
    Task<TicketResponseDto> AssignTicketAsync(Guid gId, AssignTicketDto request);
    Task<TicketResponseDto> UpdateTicketStatusAsync(Guid gId, UpdateTicketStatusDto request);
    Task DeleteTicketAsync(Guid gId);

    Task<TicketCommentResponseDto> AddCommentAsync(Guid gId, CreateTicketCommentDto request);
    Task<IEnumerable<TicketCommentResponseDto>> GetCommentsAsync(Guid gId);
    Task<IEnumerable<TicketHistoryResponseDto>> GetHistoryAsync(Guid gId);

    Task<ResolveHub.Core.DTOs.Sla.TicketSlaResponseDto?> GetTicketSlaAsync(Guid gId);
}
