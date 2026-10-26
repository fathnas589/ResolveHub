import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toastService = inject(ToastService);

  const allowedRoles = (route.data?.['roles'] as string[]) || [];
  const currentRole = authService.currentUser()?.role;

  // If no specific roles required, or user has an allowed role, grant access
  if (allowedRoles.length === 0 || (currentRole && allowedRoles.includes(currentRole))) {
    return true;
  }

  // Unauthorized navigation handling:
  // USER accessing Admin route -> redirect to /user/dashboard (never /tickets)
  if (currentRole === 'USER') {
    toastService.warning(
      'Access Restricted',
      'Your requester account does not have access to administrative management modules.'
    );
    return router.parseUrl('/user/dashboard');
  }

  // ADMIN or AGENT accessing USER-only route -> redirect to /dashboard
  if (currentRole === 'ADMIN' || currentRole === 'AGENT') {
    toastService.warning(
      'Access Restricted',
      'Administrative accounts cannot access user portal routes.'
    );
    return router.parseUrl('/dashboard');
  }

  // Unauthenticated fallback
  return router.parseUrl('/login');
};
