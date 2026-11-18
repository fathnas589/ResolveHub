import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const toastService = inject(ToastService);
  const token = authService.token();

  let authReq = req;
  if (token && !req.headers.has('Authorization')) {
    authReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${token}`)
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Only treat a 401 as "session expired" if this request actually carried the current token.
      // This avoids a bogus toast for anonymous requests and stops parallel 401s from each
      // logging out / toasting / navigating (after the first one, the token no longer matches).
      if (
        error.status === 401 &&
        !req.url.includes('/api/Auth/login') &&
        token &&
        authService.token() === token
      ) {
        authService.logout();
        toastService.error('Your session has expired. Please sign in again.');
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};

