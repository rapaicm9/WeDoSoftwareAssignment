import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import { environment } from '../../../../environments/environment';
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

  getWorkouts(): Observable<readonly WorkoutResponse[]> {
    return this.http.get<readonly WorkoutResponse[]>(this.apiUrl).pipe(
      catchError((error: unknown) => {
        console.error('Failed to load workouts.', error);
        return throwError(() => error);
      }),
    );
  }

  deleteWorkout(workoutId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${workoutId}`).pipe(
      catchError((error: unknown) => {
        console.error('Failed to delete workout.', error);
        return throwError(() => error);
      }),
    );
  }
}