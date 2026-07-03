import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
} from './auth-api.models';

@Injectable({
  providedIn: 'root',
})
export class AuthApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly authApiUrl = `${environment.apiBaseUrl}/auth`;

  public login(request: LoginRequest): Observable<LoginResponse> {
    return this.httpClient
      .post<LoginResponse>(`${this.authApiUrl}/login`, request)
      .pipe(
        catchError((error: HttpErrorResponse) =>
          this.handleError(error, 'Login'),
        ),
      );
  }

  public register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.httpClient
      .post<RegisterResponse>(`${this.authApiUrl}/register`, request)
      .pipe(
        catchError((error: HttpErrorResponse) =>
          this.handleError(error, 'Register'),
        ),
      );
  }

  private handleError(
    error: HttpErrorResponse,
    operationName: string,
  ): Observable<never> {
    console.error(`${operationName} request failed.`, error);

    return throwError(() => error);
  }
}