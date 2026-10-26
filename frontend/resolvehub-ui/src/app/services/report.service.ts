import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import { PerformanceReportResponse } from '../models/report.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/Report`;

  getPerformance(fromDateUtc?: string, toDateUtc?: string): Observable<PerformanceReportResponse> {
    let params = new HttpParams();
    if (fromDateUtc) {
      params = params.set('fromDateUtc', fromDateUtc);
    }
    if (toDateUtc) {
      params = params.set('toDateUtc', toDateUtc);
    }
    return this.http.get<PerformanceReportResponse>(`${this.baseUrl}/performance`, { params });
  }

  exportCsv(fromDateUtc?: string, toDateUtc?: string): Observable<Blob> {
    let params = new HttpParams();
    if (fromDateUtc) {
      params = params.set('fromDateUtc', fromDateUtc);
    }
    if (toDateUtc) {
      params = params.set('toDateUtc', toDateUtc);
    }
    return this.http.get(`${this.baseUrl}/export-csv`, {
      params,
      responseType: 'blob'
    });
  }
}
