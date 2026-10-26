import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import {
  SlaPolicy,
  CreateSlaPolicyRequest,
  UpdateSlaPolicyRequest,
  SlaPolicyListRequest
} from '../models/sla-policy.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class SlaPolicyService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/SlaPolicy`;

  getPolicies(request: SlaPolicyListRequest): Observable<PagedResult<SlaPolicy>> {
    let params = new HttpParams()
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.searchTerm && request.searchTerm.trim() !== '') {
      params = params.set('searchTerm', request.searchTerm.trim());
    }

    if (request.departmentGId) {
      params = params.set('departmentGId', request.departmentGId);
    }

    if (request.categoryGId) {
      params = params.set('categoryGId', request.categoryGId);
    }

    if (request.priority) {
      params = params.set('priority', request.priority);
    }

    if (request.ticketType) {
      params = params.set('ticketType', request.ticketType);
    }

    if (request.isActive !== undefined) {
      params = params.set('isActive', request.isActive.toString());
    }

    return this.http.get<PagedResult<SlaPolicy>>(this.baseUrl, { params });
  }

  getPolicyByGId(gId: string): Observable<SlaPolicy> {
    return this.http.get<SlaPolicy>(`${this.baseUrl}/${gId}`);
  }

  createPolicy(payload: CreateSlaPolicyRequest): Observable<SlaPolicy> {
    return this.http.post<SlaPolicy>(this.baseUrl, payload);
  }

  updatePolicy(gId: string, payload: UpdateSlaPolicyRequest): Observable<SlaPolicy> {
    return this.http.put<SlaPolicy>(`${this.baseUrl}/${gId}`, payload);
  }

  deletePolicy(gId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${gId}`);
  }
}
