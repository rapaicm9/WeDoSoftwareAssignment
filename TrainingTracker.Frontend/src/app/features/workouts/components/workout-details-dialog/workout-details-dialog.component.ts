import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';

import { WorkoutResponse } from '../../data-access/workout.models';

export interface WorkoutDetailsDialogData {
  readonly workout: WorkoutResponse;
}

@Component({
  selector: 'app-workout-details-dialog',
  imports: [
    DatePipe,
    MatButtonModule,
    MatDialogModule,
    MatDividerModule,
  ],
  templateUrl: './workout-details-dialog.component.html',
  styleUrl: './workout-details-dialog.component.scss',
})
export class WorkoutDetailsDialogComponent {
  private readonly dialogRef = inject<
    MatDialogRef<WorkoutDetailsDialogComponent>
  >(MatDialogRef);

  private readonly data = inject<WorkoutDetailsDialogData>(MAT_DIALOG_DATA);

  protected readonly workout = this.data.workout;
  protected readonly notes = this.workout.notes?.trim();

  protected close(): void {
    this.dialogRef.close();
  }

  protected formatWorkoutType(workoutType: string): string {
    return workoutType.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  protected formatDuration(totalMinutes: number): string {
    if (totalMinutes < 60) {
      return `${totalMinutes} min`;
    }

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    if (minutes === 0) {
      return `${hours} h`;
    }

    return `${hours} h ${minutes} min`;
  }
}