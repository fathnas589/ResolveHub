namespace ResolveHub.Core.Interfaces.Repositories;

using System;
using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Reports;

public interface IReportRepository
{
    Task<PerformanceReportResponseDto> GetPerformanceReportAsync(int companyId, DateTime? fromDateUtc, DateTime? toDateUtc);
}
