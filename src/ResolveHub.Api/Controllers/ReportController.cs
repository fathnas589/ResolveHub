namespace ResolveHub.Api.Controllers;

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ResolveHub.Core.DTOs.Reports;
using ResolveHub.Core.Interfaces.Repositories;
using ResolveHub.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN,AGENT")]
public class ReportController : ControllerBase
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentUserService _currentUserService;

    public ReportController(
        IReportRepository reportRepository,
        ICurrentUserService currentUserService)
    {
        _reportRepository = reportRepository;
        _currentUserService = currentUserService;
    }

    [HttpGet("performance")]
    [ProducesResponseType(typeof(PerformanceReportResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformance(
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc)
    {
        int companyId = _currentUserService.CompanyId;
        if (companyId <= 0)
        {
            companyId = 1;
        }

        var report = await _reportRepository.GetPerformanceReportAsync(companyId, fromDateUtc, toDateUtc);
        return Ok(report);
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc)
    {
        int companyId = _currentUserService.CompanyId;
        if (companyId <= 0)
        {
            companyId = 1;
        }

        var report = await _reportRepository.GetPerformanceReportAsync(companyId, fromDateUtc, toDateUtc);

        var sb = new StringBuilder();
        sb.AppendLine("ResolveHub Executive SLA & Performance Audit Report");
        sb.AppendLine($"Generated: {DateTime.UtcNow:u}");
        sb.AppendLine($"Time Period: {fromDateUtc:d} to {toDateUtc:d}");
        sb.AppendLine();

        sb.AppendLine("=== EXECUTIVE SUMMARY ===");
        sb.AppendLine("Total Tickets,Resolved Tickets,Compliant Tickets,Breached Tickets,Compliance %,Avg Response Hours,Avg Resolution Hours");
        sb.AppendLine($"{report.Summary.TotalTickets},{report.Summary.ResolvedTickets},{report.Summary.CompliantTickets},{report.Summary.BreachedTickets},{report.Summary.CompliancePercentage}%,{report.Summary.AvgFirstResponseHours:F1},{report.Summary.AvgResolutionHours:F1}");
        sb.AppendLine();

        sb.AppendLine("=== DEPARTMENT PERFORMANCE ===");
        sb.AppendLine("Department,Total Tickets,Resolved Count,Breached Count,Compliance %,Avg Resolution Hours");
        foreach (var dept in report.Departments)
        {
            sb.AppendLine($"\"{dept.DepartmentName}\",{dept.TotalTickets},{dept.ResolvedCount},{dept.BreachedCount},{dept.ComplianceRate}%,{dept.AvgResolutionHours:F1}");
        }
        sb.AppendLine();

        sb.AppendLine("=== AGENT PERFORMANCE ===");
        sb.AppendLine("Agent Name,Email,Assigned Count,Resolved Count,Breached Count,Compliance %");
        foreach (var agent in report.Agents)
        {
            sb.AppendLine($"\"{agent.AgentName}\",\"{agent.AgentEmail}\",{agent.AssignedCount},{agent.ResolvedCount},{agent.BreachedCount},{agent.ComplianceRate}%");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"ResolveHub_PerformanceReport_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
