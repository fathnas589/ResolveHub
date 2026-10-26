namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Dashboard;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class DashboardRepository : IDashboardRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DashboardRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DashboardMetricsResponseDto> GetMetricsAsync(int companyId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);

        using var multi = await connection.QueryMultipleAsync(
            "dbo.Dashboard_GetMetrics",
            parameters,
            commandType: CommandType.StoredProcedure);

        var summary = (await multi.ReadFirstOrDefaultAsync<DashboardSummaryDto>()) ?? new DashboardSummaryDto();

        var statusRows = await multi.ReadAsync<dynamic>();
        var statusBreakdown = statusRows.Select(r => new DashboardDistributionItemDto
        {
            Name = (string)r.Status,
            Count = (int)r.TicketCount
        }).ToList();

        var priorityRows = await multi.ReadAsync<dynamic>();
        var priorityBreakdown = priorityRows.Select(r => new DashboardDistributionItemDto
        {
            Name = (string)r.Priority,
            Count = (int)r.TicketCount
        }).ToList();

        var deptRows = await multi.ReadAsync<dynamic>();
        var deptBreakdown = deptRows.Select(r => new DashboardDistributionItemDto
        {
            Name = (string)r.DepartmentName,
            Count = (int)r.TicketCount
        }).ToList();

        var recentTickets = (await multi.ReadAsync<DashboardRecentTicketDto>()).ToList();

        return new DashboardMetricsResponseDto
        {
            Summary = summary,
            StatusBreakdown = statusBreakdown,
            PriorityBreakdown = priorityBreakdown,
            DepartmentBreakdown = deptBreakdown,
            RecentTickets = recentTickets
        };
    }
}
