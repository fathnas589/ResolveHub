export interface ReportSummary {
  totalTickets: number;
  resolvedTickets: number;
  breachedTickets: number;
  compliantTickets: number;
  compliancePercentage: number;
  avgFirstResponseHours: number;
  avgResolutionHours: number;
}

export interface DepartmentPerformance {
  departmentName: string;
  totalTickets: number;
  resolvedCount: number;
  breachedCount: number;
  complianceRate: number;
  avgResolutionHours: number;
}

export interface AgentPerformance {
  agentName: string;
  agentEmail: string;
  assignedCount: number;
  resolvedCount: number;
  breachedCount: number;
  complianceRate: number;
}

export interface PerformanceReportResponse {
  summary: ReportSummary;
  departments: DepartmentPerformance[];
  agents: AgentPerformance[];
}
