import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of, shareReplay } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import { TicketPriority, TicketType } from '../models/ticket.model';

export interface TriageSuggestion {
  departmentGId: string;
  departmentName: string;
  categoryGId: string;
  categoryName: string;
  subCategoryGId: string | null;
  subCategoryName: string | null;
  priority: TicketPriority;
  ticketType: TicketType;
  reason: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/ai`;

  /** Whether the backend has AI configured. Cached for the session; false on any error. */
  readonly enabled$: Observable<boolean> = this.http.get<{ enabled: boolean }>(`${this.baseUrl}/status`).pipe(
    map(res => !!res?.enabled),
    catchError(() => of(false)),
    shareReplay(1)
  );

  suggestTriage(subject: string, description: string): Observable<TriageSuggestion> {
    return this.http.post<TriageSuggestion>(`${this.baseUrl}/triage-suggestion`, { subject, description });
  }
}
