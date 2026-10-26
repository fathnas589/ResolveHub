import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { provideRouter } from '@angular/router';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { authGuard } from './auth.guard';
import { roleGuard } from './role.guard';
import { rootRedirect, routes } from '../../app.routes';
import { AuthService, UserProfile } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

describe('Phase B - Routing & Guards Foundation', () => {
  let authService: AuthService;
  let toastService: ToastService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter(routes),
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
        ToastService
      ]
    });

    authService = TestBed.inject(AuthService);
    toastService = TestBed.inject(ToastService);
    router = TestBed.inject(Router);
    vi.spyOn(toastService, 'warning').mockImplementation(() => {});
  });

  afterEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  describe('authGuard', () => {
    it('should allow navigation when token is present', () => {
      authService.token.set('valid-test-token');
      const route = {} as ActivatedRouteSnapshot;
      const state = { url: '/dashboard' } as RouterStateSnapshot;

      const result = TestBed.runInInjectionContext(() => authGuard(route, state));
      expect(result).toBe(true);
    });

    it('should redirect unauthenticated users to /login with returnUrl', () => {
      authService.token.set(null);
      const route = {} as ActivatedRouteSnapshot;
      const state = { url: '/user/dashboard' } as RouterStateSnapshot;

      const result = TestBed.runInInjectionContext(() => authGuard(route, state));
      expect(result).toEqual(router.createUrlTree(['/login'], { queryParams: { returnUrl: '/user/dashboard' } }));
    });
  });

  describe('roleGuard', () => {
    it('should allow ADMIN to access route with ADMIN role', () => {
      authService.currentUser.set({ role: 'ADMIN' } as UserProfile);
      const route = { data: { roles: ['ADMIN'] } } as unknown as ActivatedRouteSnapshot;

      const result = TestBed.runInInjectionContext(() => roleGuard(route, {} as RouterStateSnapshot));
      expect(result).toBe(true);
    });

    it('should allow USER to access route with USER role', () => {
      authService.currentUser.set({ role: 'USER' } as UserProfile);
      const route = { data: { roles: ['USER'] } } as unknown as ActivatedRouteSnapshot;

      const result = TestBed.runInInjectionContext(() => roleGuard(route, {} as RouterStateSnapshot));
      expect(result).toBe(true);
    });

    it('should redirect unauthorized USER accessing ADMIN route to /user/dashboard (never /tickets)', () => {
      authService.currentUser.set({ role: 'USER' } as UserProfile);
      const route = { data: { roles: ['ADMIN'] } } as unknown as ActivatedRouteSnapshot;

      const result = TestBed.runInInjectionContext(() => roleGuard(route, {} as RouterStateSnapshot));
      expect(result).toEqual(router.parseUrl('/user/dashboard'));
      expect(toastService.warning).toHaveBeenCalled();
    });

    it('should redirect unauthorized ADMIN accessing USER route to /dashboard', () => {
      authService.currentUser.set({ role: 'ADMIN' } as UserProfile);
      const route = { data: { roles: ['USER'] } } as unknown as ActivatedRouteSnapshot;

      const result = TestBed.runInInjectionContext(() => roleGuard(route, {} as RouterStateSnapshot));
      expect(result).toEqual(router.parseUrl('/dashboard'));
    });
  });

  describe('rootRedirect', () => {
    it('should redirect unauthenticated user to /login', () => {
      authService.token.set(null);
      authService.currentUser.set(null);

      const target = TestBed.runInInjectionContext(() => rootRedirect());
      expect(target).toBe('/login');
    });

    it('should redirect authenticated USER to /user/dashboard', () => {
      authService.token.set('valid-token');
      authService.currentUser.set({ role: 'USER' } as UserProfile);

      const target = TestBed.runInInjectionContext(() => rootRedirect());
      expect(target).toBe('/user/dashboard');
    });

    it('should redirect authenticated ADMIN to /dashboard', () => {
      authService.token.set('valid-token');
      authService.currentUser.set({ role: 'ADMIN' } as UserProfile);

      const target = TestBed.runInInjectionContext(() => rootRedirect());
      expect(target).toBe('/dashboard');
    });

    it('should redirect authenticated AGENT to /dashboard', () => {
      authService.token.set('valid-token');
      authService.currentUser.set({ role: 'AGENT' } as UserProfile);

      const target = TestBed.runInInjectionContext(() => rootRedirect());
      expect(target).toBe('/dashboard');
    });
  });
});
