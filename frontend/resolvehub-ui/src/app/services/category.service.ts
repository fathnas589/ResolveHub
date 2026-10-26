import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../core/config/api.config';
import {
  Category,
  CategoryListRequest,
  CreateCategoryRequest,
  UpdateCategoryRequest
} from '../models/category.model';
import { PagedResult } from '../models/paged-result.model';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_CONFIG.baseUrl}/Category`;

  getCategories(
    request: Partial<CategoryListRequest> = {},
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<PagedResult<Category>> {
    let params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('pageNumber', (request.pageNumber || 1).toString())
      .set('pageSize', (request.pageSize || 100).toString());

    if (request.searchTerm) {
      params = params.set('searchTerm', request.searchTerm.trim());
    }

    if (request.departmentGId) {
      params = params.set('departmentGId', request.departmentGId);
    }

    if (request.isActive !== null && request.isActive !== undefined) {
      params = params.set('isActive', request.isActive.toString());
    }

    return this.http.get<PagedResult<Category>>(this.baseUrl, { params });
  }

  getCategoryByGId(
    gId: string,
    companyId = API_CONFIG.defaultCompanyId
  ): Observable<Category> {
    const params = new HttpParams().set('companyId', companyId.toString());
    return this.http.get<Category>(`${this.baseUrl}/${gId}`, { params });
  }

  createCategory(
    payload: CreateCategoryRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<Category> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    return this.http.post<Category>(this.baseUrl, payload, { params });
  }

  updateCategory(
    gId: string,
    payload: UpdateCategoryRequest,
    companyId = API_CONFIG.defaultCompanyId,
    userId = API_CONFIG.defaultUserId
  ): Observable<Category> {
    const params = new HttpParams()
      .set('companyId', companyId.toString())
      .set('userId', userId.toString());
    return this.http.put<Category>(`${this.baseUrl}/${gId}`, payload, { params });
  }

  deleteCategory(
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
