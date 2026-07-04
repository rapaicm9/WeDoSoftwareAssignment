import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TokenStorageService } from '../auth/token-storage.service';
import { getApiErrorCode } from './api-error-message';

const invalidCurrentPasswordErrorCode = 'Users.InvalidCurrentPassword';
const inactiveUserErrorCode = 'Users.UserInactive';

export const authTokenInterceptor: HttpInterceptorFn = (request, next) => {
  const router = inject(Router);
  const tokenStorageService = inject(TokenStorageService);

  const isApiRequest = request.url.startsWith(environment.apiBaseUrl);
  const authorizationHeaderValue =
    tokenStorageService.getAuthorizationHeaderValue();

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
      if (error instanceof HttpErrorResponse && isApiRequest) {
        const errorCode = getApiErrorCode(error);

        if (
          error.status === 401 &&
          errorCode !== invalidCurrentPasswordErrorCode
        ) {
          console.warn('Unauthorized API response received.');
          clearSessionAndRedirectToLogin(tokenStorageService, router);
        }

        if (error.status === 403 && errorCode === inactiveUserErrorCode) {
          console.warn('Inactive user API response received.');
          clearSessionAndRedirectToLogin(tokenStorageService, router);
        }
      }

      return throwError(() => error);
    }),
  );
};

function clearSessionAndRedirectToLogin(
  tokenStorageService: TokenStorageService,
  router: Router,
): void {
  tokenStorageService.clearSession();
  void router.navigate(['/login']);
}