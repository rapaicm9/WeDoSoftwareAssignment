import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../../environments/environment';

import { Observable, catchError, throwError } from 'rxjs';

import {
  AddWorkoutRequest,
  WorkoutResponse,
} from './workout.models';

@Injectable({
  providedIn: 'root',
})
export class WorkoutsApiService {
  private readonly http = inject(HttpClient);

  private readonly apiUrl = `${environment.apiBaseUrl}/workouts`;

  addWorkout(request: AddWorkoutRequest): Observable<WorkoutResponse> {
    return this.http.post<WorkoutResponse>(this.apiUrl, request).pipe(
      catchError((error: unknown) => {
        console.error('Failed to add workout.', error);

        return throwError(() => error);
      }),
    );
  }
}