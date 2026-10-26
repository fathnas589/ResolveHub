namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Sla;
using ResolveHub.Core.Entities;

public interface ITicketSlaRepository
{
    Task<TicketSla?> InsertAsync(TicketSla ticketSla);
    Task<TicketSla?> GetByTicketIdAsync(int companyId, int ticketId);
    Task<(int RowsAffected, bool FirstResponseBreached, DateTime FirstResponseDueAt)> UpdateFirstResponseAsync(int ticketId, DateTime firstRespondedAt);
    Task<TicketSla?> UpdateResolutionAsync(int ticketId, string status, DateTime eventTime);
    Task<IEnumerable<BreachedTicketSlaEvent>> CheckBreachesAsync(DateTime currentUtc);
    Task<IEnumerable<WarningTicketSlaEvent>> CheckWarningsAsync(DateTime currentUtc);
}
