import { TicketSlaSummary } from './sla-policy.model';

export type TicketType = 'COMPLAINT' | 'SERVICE_REQUEST';

export type TicketPriority = 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';

export type TicketStatus =
  | 'OPEN'
  | 'ASSIGNED'
  | 'IN_PROGRESS'
  | 'PENDING_USER'
  | 'RESOLVED'
  | 'CLOSED'
  | 'REJECTED';

export interface Ticket {
  gId: string;
  ticketNumber: string;
  ticketType: TicketType;
  subject: string;
  description: string;
  priority: TicketPriority;
  status: TicketStatus;
  departmentGId: string;
  departmentName: string;
  categoryGId: string;
  categoryName: string;
  subCategoryGId: string | null;
  subCategoryName: string | null;
  createdByUserGId: string;
  createdByUserName: string;
  createdByFullName: string;
  assignedToUserGId: string | null;
  assignedToUserName: string | null;
  assignedToFullName: string | null;
  created: string;
  lastUpd: string | null;
  closedAt: string | null;
  sla?: TicketSlaSummary | null;
}

export interface CreateTicketRequest {
  ticketType: TicketType;
  departmentGId: string;
  categoryGId: string;
  subCategoryGId?: string | null;
  subject: string;
  description: string;
  priority: TicketPriority;
}

export interface UpdateTicketRequest {
  departmentGId: string;
  categoryGId: string;
  subCategoryGId?: string | null;
  subject: string;
  description: string;
  priority: TicketPriority;
}

export interface AssignTicketRequest {
  assignedToUserGId: string;
  comments?: string;
}

export interface UpdateTicketStatusRequest {
  status: TicketStatus;
  comments?: string;
}

export interface TicketListRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  status?: TicketStatus | string;
  priority?: TicketPriority | string;
  departmentGId?: string;
  ticketType?: TicketType | string;
  assignedToUserGId?: string;
  myTicketsOnly?: boolean;
}

export interface TicketComment {
  gId: string;
  authorUserGId: string;
  authorUserName: string;
  authorFullName: string;
  authorRoleCode: string;
  commentText: string;
  isInternal: boolean;
  created: string;
}

export interface CreateTicketCommentRequest {
  commentText: string;
  isInternal: boolean;
}

export interface TicketHistory {
  gId: string;
  actionType: string;
  actorUserGId: string;
  actorUserName: string;
  actorFullName: string;
  oldValue: string | null;
  newValue: string | null;
  comments: string | null;
  created: string;
}

/**
 * State machine of valid status transitions. Mirrors TicketService.ValidateStatusTransition on the backend.
 * OPEN -> ASSIGNED is intentionally absent: assignment goes through the Assign action so an assignee is always set.
 */
export const VALID_STATUS_TRANSITIONS: Record<TicketStatus, TicketStatus[]> = {
  OPEN: ['REJECTED'],
  ASSIGNED: ['IN_PROGRESS', 'REJECTED'],
  IN_PROGRESS: ['PENDING_USER', 'RESOLVED'],
  PENDING_USER: ['IN_PROGRESS', 'RESOLVED'],
  RESOLVED: ['CLOSED', 'IN_PROGRESS'],
  CLOSED: ['OPEN'],
  REJECTED: ['OPEN']
};

/** Transitions only an ADMIN may perform (reopening closed/rejected tickets). */
const ADMIN_ONLY_TRANSITIONS: ReadonlySet<string> = new Set(['CLOSED->OPEN', 'REJECTED->OPEN']);

/** Returns the statuses the given role may move a ticket to from its current status. */
export function getAllowedNextStatuses(current: TicketStatus, role: string | null | undefined): TicketStatus[] {
  const normalizedRole = (role || '').toUpperCase();
  return (VALID_STATUS_TRANSITIONS[current] || []).filter(
    next => normalizedRole === 'ADMIN' || !ADMIN_ONLY_TRANSITIONS.has(`${current}->${next}`)
  );
}
