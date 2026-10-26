import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import { DashboardMetricsResponse } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/Dashboard`;

  getMetrics(): Observable<DashboardMetricsResponse> {
    return this.http.get<DashboardMetricsResponse>(`${this.baseUrl}/metrics`);
  }
}
