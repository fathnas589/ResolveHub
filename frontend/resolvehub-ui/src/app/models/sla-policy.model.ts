export type SlaHealthStatus =
  | 'ON_TRACK'
  | 'WARNING'
  | 'FIRST_RESPONSE_BREACHED'
  | 'RESOLUTION_BREACHED'
  | 'COMPLETED_ON_TIME'
  | 'COMPLETED_BREACHED'
  | 'NO_SLA';

export interface SlaPolicy {
  gId: string;
  name: string;
  description?: string | null;
  departmentGId?: string | null;
  departmentName?: string | null;
  categoryGId?: string | null;
  categoryName?: string | null;
  subCategoryGId?: string | null;
  subCategoryName?: string | null;
  priority?: string | null;
  ticketType?: string | null;
  firstResponseMinutes: number;
  resolutionMinutes: number;
  escalationBeforeMinutes: number;
  isDefault: boolean;
  isActive: boolean;
  created: string;
  createdByName?: string | null;
  lastUpd?: string | null;
  lastUpdByName?: string | null;
}

export interface CreateSlaPolicyRequest {
  name: string;
  description?: string | null;
  departmentGId?: string | null;
  categoryGId?: string | null;
  subCategoryGId?: string | null;
  priority?: string | null;
  ticketType?: string | null;
  firstResponseMinutes: number;
  resolutionMinutes: number;
  escalationBeforeMinutes: number;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdateSlaPolicyRequest {
  name: string;
  description?: string | null;
  departmentGId?: string | null;
  categoryGId?: string | null;
  subCategoryGId?: string | null;
  priority?: string | null;
  ticketType?: string | null;
  firstResponseMinutes: number;
  resolutionMinutes: number;
  escalationBeforeMinutes: number;
  isDefault: boolean;
  isActive: boolean;
}

export interface SlaPolicyListRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  departmentGId?: string;
  categoryGId?: string;
  priority?: string;
  ticketType?: string;
  isActive?: boolean;
}

export interface TicketSlaSummary {
  slaPolicyName?: string | null;
  slaHealth: SlaHealthStatus | string;
  firstResponseDueAt?: string | null;
  resolutionDueAt?: string | null;
  firstResponseBreached: boolean;
  resolutionBreached: boolean;
  firstResponseRemainingSeconds?: number | null;
  resolutionRemainingSeconds?: number | null;
}

export interface TicketSla {
  gId: string;
  slaPolicyGId?: string | null;
  slaPolicyName?: string | null;
  firstResponseMinutes: number;
  resolutionMinutes: number;
  firstResponseDueAt: string;
  firstRespondedAt?: string | null;
  firstResponseBreached: boolean;
  resolutionDueAt: string;
  resolvedAt?: string | null;
  resolutionBreached: boolean;
  escalationDueAt?: string | null;
  escalationTriggeredAt?: string | null;
  slaHealth: SlaHealthStatus | string;
  firstResponseRemainingSeconds?: number | null;
  resolutionRemainingSeconds?: number | null;
  created: string;
  lastUpd?: string | null;
}
