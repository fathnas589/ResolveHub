namespace ResolveHub.Api.Controllers;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Dashboard;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICurrentUserService _currentUserService;

    public DashboardController(
        IDashboardRepository dashboardRepository,
        ICurrentUserService currentUserService)
    {
        _dashboardRepository = dashboardRepository;
        _currentUserService = currentUserService;
    }

    [HttpGet("metrics")]
    [Authorize(Roles = "ADMIN,AGENT")]
    [ProducesResponseType(typeof(DashboardMetricsResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics()
    {
        int companyId = _currentUserService.CompanyId;
        if (companyId <= 0)
        {
            companyId = 1;
        }

        var metrics = await _dashboardRepository.GetMetricsAsync(companyId);
        return Ok(metrics);
    }
}
