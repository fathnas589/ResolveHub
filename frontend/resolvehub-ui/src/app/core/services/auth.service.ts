import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { API_CONFIG } from '../config/api.config';

export interface UserProfile {
  gId: string;
  userName: string;
  fullName: string;
  email: string;
  role: 'ADMIN' | 'AGENT' | 'USER';
  departmentGId?: string;
  departmentName?: string;
  companyId: number;
}

export interface LoginResponse {
  token: string;
  user: UserProfile;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  phone?: string;
  password: string;
  companyId?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly authUrl = `${API_CONFIG.baseUrl}/Auth`;

  // Reactive signals for current user and token
  readonly token = signal<string | null>(localStorage.getItem('rh_token'));
  readonly currentUser = signal<UserProfile | null>(this.getStoredUser());

  private normalizeUser(user: any, token?: string): UserProfile {
    let role = (user?.role || user?.roleCode || '').toUpperCase();
    if (!role && token) {
      try {
        const parts = token.split('.');
        if (parts.length >= 2) {
          const payload = JSON.parse(atob(parts[1]));
          role = (payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '').toUpperCase();
        }
      } catch {
        // ignore decoding errors
      }
    }
    if (!role) {
      role = 'USER';
    }

    return {
      ...user,
      role: role as 'ADMIN' | 'AGENT' | 'USER',
      roleCode: user?.roleCode || role
    };
  }

  private getStoredUser(): UserProfile | null {
    const raw = localStorage.getItem('rh_user');
    const token = localStorage.getItem('rh_token');
    if (!raw) return null;
    try {
      const parsed = JSON.parse(raw);
      return this.normalizeUser(parsed, token || undefined);
    } catch {
      return null;
    }
  }

  login(userNameOrEmail: string, password: string, companyId = 1): Observable<LoginResponse> {
    return this.http.post<any>(`${this.authUrl}/login`, {
      companyId,
      userNameOrEmail,
      password
    }).pipe(
      tap(res => {
        const user = this.normalizeUser(res.user, res.token);
        this.setSession(res.token, user);
      }),
      // Return response with normalized user so downstream subscribers get valid role
      tap(res => {
        res.user = this.normalizeUser(res.user, res.token);
      })
    );
  }

  getMe(): Observable<UserProfile> {
    return this.http.get<any>(`${this.authUrl}/me`).pipe(
      tap(user => {
        const normalized = this.normalizeUser(user, this.token() || undefined);
        this.currentUser.set(normalized);
        localStorage.setItem('rh_user', JSON.stringify(normalized));
      })
    );
  }

  register(payload: RegisterRequest): Observable<LoginResponse> {
    // Note: Registration does NOT set session because newly registered users require admin approval
    return this.http.post<LoginResponse>(`${this.authUrl}/register`, {
      companyId: payload.companyId ?? 1,
      fullName: payload.fullName,
      email: payload.email,
      phone: payload.phone || null,
      password: payload.password
    });
  }

  setSession(token: string, rawUser: any): void {
    const user = this.normalizeUser(rawUser, token);
    this.token.set(token);
    this.currentUser.set(user);
    localStorage.setItem('rh_token', token);
    localStorage.setItem('rh_user', JSON.stringify(user));
  }

  logout(): void {
    this.token.set(null);
    this.currentUser.set(null);
    localStorage.removeItem('rh_token');
    localStorage.removeItem('rh_user');
    localStorage.removeItem('rh_persona_id');
  }

  isAdmin(): boolean {
    return this.currentUser()?.role === 'ADMIN';
  }

  isAgent(): boolean {
    return this.currentUser()?.role === 'AGENT';
  }

  isStaff(): boolean {
    const role = this.currentUser()?.role;
    return role === 'ADMIN' || role === 'AGENT';
  }

  isRegularUser(): boolean {
    return this.currentUser()?.role === 'USER';
  }
}
