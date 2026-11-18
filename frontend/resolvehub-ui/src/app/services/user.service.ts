import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import {
  User,
  UserListRequest,
  CreateUserRequest,
  UpdateUserRequest,
  ChangePasswordRequest
} from '../models/user.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/User`;

  getUsers(
    request: Partial<UserListRequest> = {},
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<PagedResult<User>> {
    let params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('pageNumber', (request.pageNumber || 1).toString())
      .set('pageSize', (request.pageSize || 100).toString());

    if (request.searchTerm) {
      params = params.set('searchTerm', request.searchTerm.trim());
    }

    if (request.roleCode) {
      params = params.set('roleCode', request.roleCode);
    }

    if (request.departmentGId) {
      params = params.set('departmentGId', request.departmentGId);
    }

    if (request.isActive !== null && request.isActive !== undefined) {
      params = params.set('isActive', request.isActive.toString());
    }

    return this.http.get<PagedResult<User>>(this.baseUrl, { params });
  }

  getUserByGId(
    gId: string,
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<User> {
    const params = new HttpParams().set('companyId', companyId.toString());
    return this.http.get<User>(`${this.baseUrl}/${gId}`, { params });
  }

  createUser(
    payload: CreateUserRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<User> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    return this.http.post<User>(this.baseUrl, payload, { params });
  }

  updateUser(
    gId: string,
    payload: UpdateUserRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<User> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    return this.http.put<User>(`${this.baseUrl}/${gId}`, payload, { params });
  }

  deleteUser(
    gId: string,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<void> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    return this.http.delete<void>(`${this.baseUrl}/${gId}`, { params });
  }

  changePassword(
    gId: string,
    newPassword: string,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<void> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    const body: ChangePasswordRequest = { newPassword };
    return this.http.put<void>(`${this.baseUrl}/${gId}/password`, body, { params });
  }

  getAgentsByDepartment(departmentGId?: string): Observable<PagedResult<User>> {
    // Uses the ADMIN+AGENT lookup endpoint; the general user listing is ADMIN-only.
    let params = new HttpParams();
    if (departmentGId) {
      params = params.set('departmentGId', departmentGId);
    }
    return this.http.get<PagedResult<User>>(`${this.baseUrl}/assignable`, { params });
  }

  approveUser(
    user: User,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<User> {
    return this.updateUser(
      user.gId,
      {
        userName: user.userName,
        fullName: user.fullName,
        email: user.email,
        phone: user.phone || undefined,
        roleCode: user.roleCode,
        departmentGId: user.departmentGId || undefined,
        isActive: true
      },
      companyId,
      userId
    );
  }
}
