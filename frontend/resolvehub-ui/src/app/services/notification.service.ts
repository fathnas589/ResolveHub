import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, of } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import {
  NotificationItem,
  NotificationListRequest,
  NotificationPagedResult,
  UnreadNotificationCount
} from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/notifications`;

  /**
   * Reactive signal holding current unread notification count
   */
  readonly unreadCount = signal<number>(0);

  /**
   * Retrieves paginated notifications with optional filters.
   */
  getNotifications(request?: NotificationListRequest): Observable<NotificationPagedResult> {
    let params = new HttpParams();

    if (request?.pageNumber) {
      params = params.set('pageNumber', request.pageNumber.toString());
    }
    if (request?.pageSize) {
      params = params.set('pageSize', request.pageSize.toString());
    }
    if (request?.unreadOnly !== undefined && request.unreadOnly !== null) {
      params = params.set('unreadOnly', request.unreadOnly.toString());
    }
    if (request?.eventType && request.eventType.trim() !== '') {
      params = params.set('eventType', request.eventType.trim());
    }

    return this.http.get<NotificationPagedResult>(this.baseUrl, { params });
  }

  /**
   * Fetches latest unread count from server and updates signal.
   */
  refreshUnreadCount(): Observable<UnreadNotificationCount> {
    return this.http.get<UnreadNotificationCount>(`${this.baseUrl}/unread-count`).pipe(
      tap(res => {
        this.unreadCount.set(res.unreadCount || 0);
      }),
      catchError(() => {
        return of({ unreadCount: this.unreadCount() });
      })
    );
  }

  /**
   * Marks a single notification as read.
   */
  markRead(gId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.baseUrl}/${gId}/read`, {}).pipe(
      tap(() => {
        this.unreadCount.update(c => Math.max(0, c - 1));
      })
    );
  }

  /**
   * Marks all notifications as read for current user.
   */
  markAllRead(): Observable<{ message: string; count: number }> {
    return this.http.post<{ message: string; count: number }>(`${this.baseUrl}/read-all`, {}).pipe(
      tap(() => {
        this.unreadCount.set(0);
      })
    );
  }
}
