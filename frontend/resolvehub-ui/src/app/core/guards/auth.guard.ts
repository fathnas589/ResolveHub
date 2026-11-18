import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.token() && !authService.isTokenExpired()) {
    return true;
  }

  // Clear any stale/expired session before redirecting to login.
  if (authService.token()) {
    authService.logout();
  }

  // Preserve intended destination as returnUrl for unauthenticated users
  return router.createUrlTree(['/login'], {
    queryParams: state.url && state.url !== '/' ? { returnUrl: state.url } : {}
  });
};
