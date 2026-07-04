import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { catchError, Observable, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { UserProfileResponse } from './profile.models';

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

  private handleError(
    error: HttpErrorResponse,
    operationName: string,
  ): Observable<never> {
    console.error(`${operationName} request failed.`, error);

    return throwError(() => error);
  }
}