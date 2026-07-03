import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { TokenStorageService } from '../auth/token-storage.service';

export const authTokenInterceptor: HttpInterceptorFn = (request, next) => {
  const tokenStorageService = inject(TokenStorageService);

  const isApiRequest = request.url.startsWith(environment.apiBaseUrl);
  const authorizationHeaderValue = tokenStorageService.getAuthorizationHeaderValue();

  const authorizedRequest =
    isApiRequest && authorizationHeaderValue
      ? request.clone({
          setHeaders: {
            Authorization: authorizationHeaderValue,
          },
        })
      : request;

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        console.warn('Unauthorized API response received.');

        if (isApiRequest) {
          tokenStorageService.clearSession();
        }
      }

      return throwError(() => error);
    }),
  );
};