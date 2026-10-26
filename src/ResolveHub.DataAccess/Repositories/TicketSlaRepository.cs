namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Sla;
using ResolveHub.Core.Entities;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class TicketSlaRepository : ITicketSlaRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public TicketSlaRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TicketSla?> InsertAsync(TicketSla ticketSla)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketId", ticketSla.TicketId, DbType.Int32);
        parameters.Add("@SlaPolicyId", ticketSla.SlaPolicyId, DbType.Int32);
        parameters.Add("@FirstResponseMinutes", ticketSla.FirstResponseMinutes, DbType.Int32);
        parameters.Add("@ResolutionMinutes", ticketSla.ResolutionMinutes, DbType.Int32);
        parameters.Add("@FirstResponseDueAt", ticketSla.FirstResponseDueAt, DbType.DateTime2);
        parameters.Add("@ResolutionDueAt", ticketSla.ResolutionDueAt, DbType.DateTime2);
        parameters.Add("@EscalationDueAt", ticketSla.EscalationDueAt, DbType.DateTime2);

        return await connection.QuerySingleOrDefaultAsync<TicketSla>(
            "dbo.TicketSla_Insert",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<TicketSla?> GetByTicketIdAsync(int companyId, int ticketId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@TicketId", ticketId, DbType.Int32);

        return await connection.QuerySingleOrDefaultAsync<TicketSla>(
            "dbo.TicketSla_GetByTicketId",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<(int RowsAffected, bool FirstResponseBreached, DateTime FirstResponseDueAt)> UpdateFirstResponseAsync(int ticketId, DateTime firstRespondedAt)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketId", ticketId, DbType.Int32);
        parameters.Add("@FirstRespondedAt", firstRespondedAt, DbType.DateTime2);

        var result = await connection.QuerySingleOrDefaultAsync<FirstResponseUpdateDbRow>(
            "dbo.TicketSla_UpdateFirstResponse",
            parameters,
            commandType: CommandType.StoredProcedure);

        if (result == null)
        {
            return (0, false, DateTime.MinValue);
        }

        return (result.RowsAffected, result.FirstResponseBreached, result.FirstResponseDueAt);
    }

    public async Task<TicketSla?> UpdateResolutionAsync(int ticketId, string status, DateTime eventTime)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@TicketId", ticketId, DbType.Int32);
        parameters.Add("@Status", status, DbType.String, size: 30);
        parameters.Add("@EventTime", eventTime, DbType.DateTime2);

        return await connection.QuerySingleOrDefaultAsync<TicketSla>(
            "dbo.TicketSla_UpdateResolution",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<BreachedTicketSlaEvent>> CheckBreachesAsync(DateTime currentUtc)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CurrentUtc", currentUtc, DbType.DateTime2);

        return await connection.QueryAsync<BreachedTicketSlaEvent>(
            "dbo.TicketSla_CheckBreaches",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<WarningTicketSlaEvent>> CheckWarningsAsync(DateTime currentUtc)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CurrentUtc", currentUtc, DbType.DateTime2);

        return await connection.QueryAsync<WarningTicketSlaEvent>(
            "dbo.TicketSla_CheckWarnings",
            parameters,
            commandType: CommandType.StoredProcedure);
    }

    private sealed class FirstResponseUpdateDbRow
    {
        public int RowsAffected { get; set; }
        public bool FirstResponseBreached { get; set; }
        public DateTime FirstResponseDueAt { get; set; }
    }
}
