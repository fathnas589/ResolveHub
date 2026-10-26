namespace ResolveHub.Core.Interfaces.Repositories;

using System.Threading.Tasks;
using ResolveHub.Core.DTOs.Dashboard;

public interface IDashboardRepository
{
    Task<DashboardMetricsResponseDto> GetMetricsAsync(int companyId);
}
