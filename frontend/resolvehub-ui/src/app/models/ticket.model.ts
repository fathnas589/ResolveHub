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
  | 'CANCELLED'
  | 'REOPENED';

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
  ticketGId: string;
  userGId: string;
  userName: string;
  userFullName: string;
  userRole: string;
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
  ticketGId: string;
  actionType: string;
  performedByUserGId: string;
  performedByUserName: string;
  performedByFullName: string;
  oldValue: string | null;
  newValue: string | null;
  remarks: string | null;
  created: string;
}

/**
 * State machine dictionary of valid transitions
 */
export const VALID_STATUS_TRANSITIONS: Record<TicketStatus, TicketStatus[]> = {
  OPEN: ['ASSIGNED', 'CANCELLED'],
  ASSIGNED: ['IN_PROGRESS', 'ASSIGNED', 'CANCELLED'],
  IN_PROGRESS: ['PENDING_USER', 'RESOLVED', 'ASSIGNED', 'CANCELLED'],
  PENDING_USER: ['IN_PROGRESS', 'RESOLVED', 'CANCELLED'],
  RESOLVED: ['CLOSED', 'REOPENED'],
  REOPENED: ['IN_PROGRESS', 'ASSIGNED', 'CANCELLED'],
  CLOSED: [],
  CANCELLED: []
};
