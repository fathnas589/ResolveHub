export interface DashboardSummary {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedToday: number;
  breachedTickets: number;
  warningTickets: number;
  avgResponseHours: number;
}

export interface DashboardDistributionItem {
  name: string;
  count: number;
}

export interface DashboardRecentTicket {
  gId: string;
  ticketNumber: string;
  subject: string;
  status: string;
  priority: string;
  departmentName: string;
  assignedToName: string;
  created: string;
  isBreached: boolean;
}

export interface DashboardMetricsResponse {
  summary: DashboardSummary;
  statusBreakdown: DashboardDistributionItem[];
  priorityBreakdown: DashboardDistributionItem[];
  departmentBreakdown: DashboardDistributionItem[];
  recentTickets: DashboardRecentTicket[];
}
