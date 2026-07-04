import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';

import { AddWorkoutDialogComponent } from '../../components/add-workout-dialog/add-workout-dialog.component';
import { WorkoutResponse } from '../../data-access/workout.models';

@Component({
  selector: 'app-workouts-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    MatTableModule,
  ],
  templateUrl: './workouts-page.component.html',
  styleUrl: './workouts-page.component.scss',
})
export class WorkoutsPageComponent {
  private readonly dialog = inject(MatDialog);

  protected readonly workouts = signal<WorkoutResponse[]>([]);

  protected readonly displayedColumns: readonly string[] = [
    'title',
    'workoutType',
    'trainingDateTimeUtc',
    'durationMinutes',
    'caloriesBurned',
    'trainingIntensity',
    'fatigue',
  ];

  protected openAddWorkoutDialog(): void {
    const dialogRef = this.dialog.open<
      AddWorkoutDialogComponent,
      undefined,
      WorkoutResponse
    >(AddWorkoutDialogComponent, {
      width: '760px',
      maxWidth: '95vw',
      disableClose: true,
    });

    dialogRef.afterClosed().subscribe((createdWorkout) => {
      if (!createdWorkout) {
        return;
      }

      this.workouts.update((currentWorkouts) => [
        createdWorkout,
        ...currentWorkouts,
      ]);
    });
  }

  protected formatWorkoutType(workoutType: string): string {
    return workoutType.replace(/([a-z])([A-Z])/g, '$1 $2');
  }
}