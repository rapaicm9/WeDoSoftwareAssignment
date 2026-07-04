import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { catchError, Observable, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  MonthlyProgressRequest,
  MonthlyProgressResponse,
} from './monthly-progress.models';

@Injectable({
  providedIn: 'root',
})
export class MonthlyProgressApiService {
  private readonly httpClient = inject(HttpClient);

  private readonly workoutsApiUrl = `${environment.apiBaseUrl}/workouts`;

  public getMonthlyProgress(
    request: MonthlyProgressRequest,
  ): Observable<MonthlyProgressResponse> {
    const params = new HttpParams()
      .set('year', request.year)
      .set('month', request.month);

    return this.httpClient
      .get<MonthlyProgressResponse>(`${this.workoutsApiUrl}/progress/monthly`, {
        params,
      })
      .pipe(
        catchError((error: HttpErrorResponse) =>
          this.handleError(error, 'Get monthly progress'),
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