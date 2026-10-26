import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import {
  Ticket,
  CreateTicketRequest,
  UpdateTicketRequest,
  AssignTicketRequest,
  UpdateTicketStatusRequest,
  TicketListRequest,
  TicketComment,
  CreateTicketCommentRequest,
  TicketHistory
} from '../models/ticket.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/Ticket`;

  /**
   * Retrieves a paginated and filtered list of tickets.
   */
  getTickets(request: TicketListRequest): Observable<PagedResult<Ticket>> {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.searchTerm && request.searchTerm.trim() !== '') {
      params = params.set('searchTerm', request.searchTerm.trim());
    }

    if (request.status) {
      params = params.set('status', request.status);
    }

    if (request.priority) {
      params = params.set('priority', request.priority);
    }

    if (request.departmentGId) {
      params = params.set('departmentGId', request.departmentGId);
    }

    if (request.ticketType) {
      params = params.set('ticketType', request.ticketType);
    }

    if (request.assignedToUserGId) {
      params = params.set('assignedToUserGId', request.assignedToUserGId);
    }

    if (request.myTicketsOnly !== undefined) {
      params = params.set('myTicketsOnly', request.myTicketsOnly.toString());
    }

    return this.http.get<PagedResult<Ticket>>(this.baseUrl, { params });
  }

  /**
   * Retrieves a single ticket by its public GId.
   */
  getTicketByGId(gId: string): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.baseUrl}/${gId}`);
  }

  /**
   * Creates a new ticket (Complaint or Service Request).
   */
  createTicket(payload: CreateTicketRequest): Observable<Ticket> {
    return this.http.post<Ticket>(this.baseUrl, payload);
  }

  /**
   * Updates ticket subject, description, or priority.
   */
  updateTicket(gId: string, payload: UpdateTicketRequest): Observable<Ticket> {
    return this.http.put<Ticket>(`${this.baseUrl}/${gId}`, payload);
  }

  /**
   * Assigns ticket to an agent.
   */
  assignTicket(gId: string, payload: AssignTicketRequest): Observable<Ticket> {
    return this.http.put<Ticket>(`${this.baseUrl}/${gId}/assign`, payload);
  }

  /**
   * Updates the workflow status of a ticket.
   */
  updateTicketStatus(gId: string, payload: UpdateTicketStatusRequest): Observable<Ticket> {
    return this.http.put<Ticket>(`${this.baseUrl}/${gId}/status`, payload);
  }

  /**
   * Soft deletes a ticket (Admin only).
   */
  deleteTicket(gId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${gId}`);
  }

  /**
   * Gets comments for a ticket.
   */
  getComments(ticketGId: string): Observable<TicketComment[]> {
    return this.http.get<TicketComment[]>(`${this.baseUrl}/${ticketGId}/comments`);
  }

  /**
   * Adds a public or internal comment to a ticket.
   */
  addComment(ticketGId: string, payload: CreateTicketCommentRequest): Observable<TicketComment> {
    return this.http.post<TicketComment>(`${this.baseUrl}/${ticketGId}/comments`, payload);
  }

  /**
   * Gets the audit history for a ticket.
   */
  getHistory(ticketGId: string): Observable<TicketHistory[]> {
    return this.http.get<TicketHistory[]>(`${this.baseUrl}/${ticketGId}/history`);
  }

  /**
   * Gets the SLA snapshot and live health status for a ticket.
   */
  getTicketSla(ticketGId: string): Observable<import('../models/sla-policy.model').TicketSla> {
    return this.http.get<import('../models/sla-policy.model').TicketSla>(`${this.baseUrl}/${ticketGId}/sla`);
  }
}
