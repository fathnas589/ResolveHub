namespace ResolveHub.Core.DTOs.Dashboard;

using System;
using System.Collections.Generic;

public class DashboardSummaryDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedToday { get; set; }
    public int BreachedTickets { get; set; }
    public int WarningTickets { get; set; }
    public double AvgResponseHours { get; set; }
}

public class DashboardDistributionItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardRecentTicketDto
{
    public Guid GId { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string AssignedToName { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public bool IsBreached { get; set; }
}

public class DashboardMetricsResponseDto
{
    public DashboardSummaryDto Summary { get; set; } = new();
    public List<DashboardDistributionItemDto> StatusBreakdown { get; set; } = new();
    public List<DashboardDistributionItemDto> PriorityBreakdown { get; set; } = new();
    public List<DashboardDistributionItemDto> DepartmentBreakdown { get; set; } = new();
    public List<DashboardRecentTicketDto> RecentTickets { get; set; } = new();
}
