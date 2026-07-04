import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { UpdateUserRequest, UserProfileResponse } from './profile.models';

@Injectable({
  providedIn: 'root',
})
export class ProfileApiService {
  private readonly httpClient = inject(HttpClient);
  private readonly usersApiUrl = `${environment.apiBaseUrl}/users`;

  public getUserById(userId: string): Observable<UserProfileResponse> {
    return this.httpClient
      .get<UserProfileResponse>(`${this.usersApiUrl}/${userId}`)
      .pipe(
        catchError((error: HttpErrorResponse) =>
          this.handleError(error, 'Get user profile'),
        ),
      );
  }

  public updateCurrentUser(
    request: UpdateUserRequest,
  ): Observable<UserProfileResponse> {
    return this.httpClient
      .patch<UserProfileResponse>(`${this.usersApiUrl}/me`, request)
      .pipe(
        catchError((error: HttpErrorResponse) =>
          this.handleError(error, 'Update user profile'),
        ),
      );
  }

  public deactivateCurrentUser(): Observable<UserProfileResponse> {
    return this.httpClient
      .patch<UserProfileResponse>(`${this.usersApiUrl}/me/deactivate`, null)
      .pipe(
        catchError((error: HttpErrorResponse) =>
          this.handleError(error, 'Deactivate user account'),
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