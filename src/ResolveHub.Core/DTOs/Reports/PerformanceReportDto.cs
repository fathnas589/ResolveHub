namespace ResolveHub.Core.DTOs.Reports;

using System;
using System.Collections.Generic;

public class ReportSummaryDto
{
    public int TotalTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int BreachedTickets { get; set; }
    public int CompliantTickets { get; set; }
    public double CompliancePercentage { get; set; }
    public double AvgFirstResponseHours { get; set; }
    public double AvgResolutionHours { get; set; }
}

public class DepartmentPerformanceDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalTickets { get; set; }
    public int ResolvedCount { get; set; }
    public int BreachedCount { get; set; }
    public double ComplianceRate { get; set; }
    public double AvgResolutionHours { get; set; }
}

public class AgentPerformanceDto
{
    public string AgentName { get; set; } = string.Empty;
    public string AgentEmail { get; set; } = string.Empty;
    public int AssignedCount { get; set; }
    public int ResolvedCount { get; set; }
    public int BreachedCount { get; set; }
    public double ComplianceRate { get; set; }
}

public class PerformanceReportResponseDto
{
    public ReportSummaryDto Summary { get; set; } = new();
    public List<DepartmentPerformanceDto> Departments { get; set; } = new();
    public List<AgentPerformanceDto> Agents { get; set; } = new();
}
