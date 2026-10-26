export interface NotificationItem {
  id: number;
  gId: string;
  companyId: number;
  eventType: string;
  title: string;
  message: string;
  ticketId?: number;
  ticketNumber?: string;
  ticketGId?: string;
  actorUserId?: number;
  actorFullName?: string;
  actionUrl?: string;
  metadataJson?: string;
  correlationKey?: string;
  isRead: boolean;
  readAt?: string;
  created: string;
}

export interface NotificationListRequest {
  unreadOnly?: boolean;
  eventType?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface UnreadNotificationCount {
  unreadCount: number;
}

export interface NotificationPagedResult {
  items: NotificationItem[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const NotificationEventTypes = {
  TicketCreated: 'TICKET_CREATED',
  TicketAssigned: 'TICKET_ASSIGNED',
  TicketReassigned: 'TICKET_REASSIGNED',
  TicketPublicCommentAdded: 'TICKET_PUBLIC_COMMENT_ADDED',
  TicketStatusChanged: 'TICKET_STATUS_CHANGED',
  TicketResolved: 'TICKET_RESOLVED',
  TicketReopened: 'TICKET_REOPENED',
  SlaWarning: 'SLA_WARNING',
  FirstResponseSlaBreached: 'FIRST_RESPONSE_SLA_BREACHED',
  ResolutionSlaBreached: 'RESOLUTION_SLA_BREACHED'
} as const;
