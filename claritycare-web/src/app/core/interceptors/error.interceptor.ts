import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        localStorage.removeItem('claritycare_token');
        router.navigate(['/auth/login']);
      } else if (error.status === 403) {
        toast.error('Access Denied', 'You do not have permission to perform this action.');
      } else if (error.status === 0) {
        toast.error('Connection Error', 'Unable to connect to the server. Please check your network.');
      } else if (error.status >= 500) {
        toast.error('Server Error', 'An unexpected error occurred. Please try again later.');
      }
      return throwError(() => error);
    })
  );
};
