import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import {
  SubCategory,
  SubCategoryListRequest,
  CreateSubCategoryRequest,
  UpdateSubCategoryRequest
} from '../models/subcategory.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class SubCategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/SubCategory`;

  getSubCategories(
    request: Partial<SubCategoryListRequest> = {},
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<PagedResult<SubCategory>> {
    let params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('pageNumber', (request.pageNumber || 1).toString())
      .set('pageSize', (request.pageSize || 100).toString());

    if (request.searchTerm) {
      params = params.set('searchTerm', request.searchTerm.trim());
    }

    if (request.categoryGId) {
      params = params.set('categoryGId', request.categoryGId);
    }

    if (request.departmentGId) {
      params = params.set('departmentGId', request.departmentGId);
    }

    if (request.isActive !== null && request.isActive !== undefined) {
      params = params.set('isActive', request.isActive.toString());
    }

    return this.http.get<PagedResult<SubCategory>>(this.baseUrl, { params });
  }

  getSubCategoryByGId(
    gId: string,
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<SubCategory> {
    const params = new HttpParams().set('companyId', companyId.toString());
    return this.http.get<SubCategory>(`${this.baseUrl}/${gId}`, { params });
  }

  createSubCategory(
    payload: CreateSubCategoryRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<SubCategory> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    return this.http.post<SubCategory>(this.baseUrl, payload, { params });
  }

  updateSubCategory(
    gId: string,
    payload: UpdateSubCategoryRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<SubCategory> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    return this.http.put<SubCategory>(`${this.baseUrl}/${gId}`, payload, { params });
  }

  deleteSubCategory(
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
