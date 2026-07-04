import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import {
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import {
  AddWorkoutRequest,
  WORKOUT_TYPE_OPTIONS,
  WorkoutResponse,
} from '../../data-access/workout.models';
import { WorkoutsApiService } from '../../data-access/workouts-api.service';

@Component({
  selector: 'app-add-workout-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
  ],
  templateUrl: './add-workout-dialog.component.html',
  styleUrl: './add-workout-dialog.component.scss',
})
export class AddWorkoutDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<AddWorkoutDialogComponent, WorkoutResponse>);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly workoutTypeOptions = WORKOUT_TYPE_OPTIONS;
  protected readonly isSubmitting = signal(false);

  protected readonly form = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    workoutType: ['Cardio' as const, [Validators.required]],
    durationMinutes: [30, [Validators.required, Validators.min(1)]],
    caloriesBurned: [0, [Validators.required, Validators.min(0)]],
    trainingIntensity: [5, [Validators.required, Validators.min(1), Validators.max(10)]],
    fatigue: [5, [Validators.required, Validators.min(1), Validators.max(10)]],
    notes: ['', [Validators.maxLength(2000)]],
    trainingDateTimeLocal: [
      this.toDateTimeLocalInputValue(new Date()),
      [Validators.required],
    ],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.snackBar.open('Please fix the highlighted workout fields.', 'Close', {
        duration: 3000,
      });

      return;
    }

    const request = this.buildRequest();

    this.isSubmitting.set(true);

    this.workoutsApiService.addWorkout(request).subscribe({
      next: (createdWorkout) => {
        this.snackBar.open('Workout added successfully.', 'Close', {
          duration: 3000,
        });

        this.dialogRef.close(createdWorkout);
      },
      error: () => {
        this.isSubmitting.set(false);

        this.snackBar.open('Failed to add workout. Please try again.', 'Close', {
          duration: 4000,
        });
      },
    });
  }

  protected cancel(): void {
    this.dialogRef.close();
  }

  private buildRequest(): AddWorkoutRequest {
    const value = this.form.getRawValue();
    const notes = value.notes.trim();

    return {
      title: value.title.trim(),
      workoutType: value.workoutType,
      durationMinutes: value.durationMinutes,
      caloriesBurned: value.caloriesBurned,
      trainingIntensity: value.trainingIntensity,
      fatigue: value.fatigue,
      notes: notes.length > 0 ? notes : null,
      trainingDateTimeUtc: new Date(value.trainingDateTimeLocal).toISOString(),
    };
  }

  private toDateTimeLocalInputValue(date: Date): string {
    const timezoneOffsetMs = date.getTimezoneOffset() * 60_000;
    const localDate = new Date(date.getTime() - timezoneOffsetMs);

    return localDate.toISOString().slice(0, 16);
  }
}