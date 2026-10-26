import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import {
  Department,
  CreateDepartmentRequest,
  UpdateDepartmentRequest,
  DepartmentListRequest
} from '../models/department.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/Department`;

  /**
   * Retrieves a paginated list of departments for the company.
   */
  getDepartments(
    request: DepartmentListRequest,
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<PagedResult<Department>> {
    let params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('pageNumber', request.pageNumber.toString())
      .set('pageSize', request.pageSize.toString());

    if (request.searchTerm && request.searchTerm.trim() !== '') {
      params = params.set('searchTerm', request.searchTerm.trim());
    }

    if (request.isActive !== null && request.isActive !== undefined) {
      params = params.set('isActive', request.isActive.toString());
    }

    return this.http.get<PagedResult<Department>>(this.baseUrl, { params });
  }

  /**
   * Retrieves a single department by its public GId.
   */
  getDepartmentByGId(
    gId: string,
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<Department> {
    const params = new HttpParams().set('companyId', companyId.toString());
    return this.http.get<Department>(`${this.baseUrl}/${gId}`, { params });
  }

  /**
   * Creates a new department.
   */
  createDepartment(
    payload: CreateDepartmentRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<Department> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());

    return this.http.post<Department>(this.baseUrl, payload, { params });
  }

  /**
   * Updates an existing department.
   */
  updateDepartment(
    gId: string,
    payload: UpdateDepartmentRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<Department> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());

    return this.http.put<Department>(`${this.baseUrl}/${gId}`, payload, { params });
  }

  /**
   * Soft deletes a department.
   */
  deleteDepartment(
    gId: string,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<void> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());

    return this.http.delete<void>(`${this.baseUrl}/${gId}`, { params });
  }
}
