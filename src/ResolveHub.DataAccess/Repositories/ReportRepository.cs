namespace ResolveHub.DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ResolveHub.Core.DTOs.Reports;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.DataAccess.Connection;

public class ReportRepository : IReportRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ReportRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PerformanceReportResponseDto> GetPerformanceReportAsync(int companyId, DateTime? fromDateUtc, DateTime? toDateUtc)
    {
        using var connection = _connectionFactory.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId, DbType.Int32);
        parameters.Add("@FromDateUtc", fromDateUtc, DbType.DateTime2);
        parameters.Add("@ToDateUtc", toDateUtc, DbType.DateTime2);

        using var multi = await connection.QueryMultipleAsync(
            "dbo.Report_GetPerformanceMetrics",
            parameters,
            commandType: CommandType.StoredProcedure);

        var summary = (await multi.ReadFirstOrDefaultAsync<ReportSummaryDto>()) ?? new ReportSummaryDto();
        var departments = (await multi.ReadAsync<DepartmentPerformanceDto>()).ToList();
        var agents = (await multi.ReadAsync<AgentPerformanceDto>()).ToList();

        return new PerformanceReportResponseDto
        {
            Summary = summary,
            Departments = departments,
            Agents = agents
        };
    }
}
