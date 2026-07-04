import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AddWorkoutDialogComponent } from '../../components/add-workout-dialog/add-workout-dialog.component';
import {
  DeleteWorkoutConfirmationDialogComponent,
  DeleteWorkoutDialogData,
} from '../../components/delete-workout-confirmation-dialog/delete-workout-confirmation-dialog.component';
import {
  WorkoutDetailsDialogComponent,
  WorkoutDetailsDialogData,
} from '../../components/workout-details-dialog/workout-details-dialog.component';
import { WorkoutResponse } from '../../data-access/workout.models';
import { WorkoutsApiService } from '../../data-access/workouts-api.service';

@Component({
  selector: 'app-workouts-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTableModule,
    MatTooltipModule,
  ],
  templateUrl: './workouts-page.component.html',
  styleUrl: './workouts-page.component.scss',
})
export class WorkoutsPageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = inject(MatDialog);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly workouts = signal<readonly WorkoutResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly hasLoadError = signal(false);
  protected readonly deletingWorkoutIds = signal<ReadonlySet<string>>(new Set());

  protected readonly displayedColumns: readonly string[] = [
    'title',
    'workoutType',
    'trainingDateTimeUtc',
    'durationMinutes',
    'caloriesBurned',
    'trainingIntensity',
    'fatigue',
    'actions',
  ];

  public ngOnInit(): void {
    this.loadWorkouts();
  }

  protected loadWorkouts(): void {
    this.isLoading.set(true);
    this.hasLoadError.set(false);

    this.workoutsApiService
      .getWorkouts()
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (response) => {
          this.workouts.set(this.sortWorkoutsDescending(response));
        },
        error: () => {
          this.workouts.set([]);
          this.hasLoadError.set(true);

          this.snackBar.open(
            'Failed to load workouts. Please try again.',
            'Close',
            {
              duration: 4000,
            },
          );
        },
      });
  }

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

    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((createdWorkout) => {
        if (!createdWorkout) {
          return;
        }

        this.workouts.update((currentWorkouts) =>
          this.sortWorkoutsDescending([
            createdWorkout,
            ...currentWorkouts,
          ]),
        );
      });
  }

  protected openWorkoutDetailsDialog(workout: WorkoutResponse): void {
    this.dialog.open<
      WorkoutDetailsDialogComponent,
      WorkoutDetailsDialogData
    >(WorkoutDetailsDialogComponent, {
      width: '620px',
      maxWidth: '95vw',
      data: {
        workout,
      },
    });
  }

  protected openDeleteWorkoutDialog(workout: WorkoutResponse): void {
    if (this.isDeletingWorkout(workout.id)) {
      return;
    }

    const dialogRef = this.dialog.open<
      DeleteWorkoutConfirmationDialogComponent,
      DeleteWorkoutDialogData,
      boolean
    >(DeleteWorkoutConfirmationDialogComponent, {
      width: '440px',
      maxWidth: '95vw',
      data: {
        title: workout.title,
      },
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.deleteWorkout(workout);
      });
  }

  protected isDeletingWorkout(workoutId: string): boolean {
    return this.deletingWorkoutIds().has(workoutId);
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

  private deleteWorkout(workout: WorkoutResponse): void {
    this.addDeletingWorkoutId(workout.id);

    this.workoutsApiService
      .deleteWorkout(workout.id)
      .pipe(
        finalize(() => {
          this.removeDeletingWorkoutId(workout.id);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.workouts.update((currentWorkouts) =>
            currentWorkouts.filter(
              (currentWorkout) => currentWorkout.id !== workout.id,
            ),
          );

          this.snackBar.open('Workout deleted.', 'Close', {
            duration: 3000,
          });
        },
        error: () => {
          this.snackBar.open(
            'Failed to delete workout. Please try again.',
            'Close',
            {
              duration: 4000,
            },
          );
        },
      });
  }

  private addDeletingWorkoutId(workoutId: string): void {
    this.deletingWorkoutIds.update((currentWorkoutIds) => {
      return new Set([
        ...currentWorkoutIds,
        workoutId,
      ]);
    });
  }

  private removeDeletingWorkoutId(workoutId: string): void {
    this.deletingWorkoutIds.update((currentWorkoutIds) => {
      const nextWorkoutIds = new Set(currentWorkoutIds);
      nextWorkoutIds.delete(workoutId);

      return nextWorkoutIds;
    });
  }

  private sortWorkoutsDescending(
    workouts: readonly WorkoutResponse[],
  ): readonly WorkoutResponse[] {
    return [...workouts].sort((firstWorkout, secondWorkout) => {
      return (
        this.getDateTimeValue(secondWorkout.trainingDateTimeUtc) -
        this.getDateTimeValue(firstWorkout.trainingDateTimeUtc)
      );
    });
  }

  private getDateTimeValue(value: string): number {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return 0;
    }

    return date.getTime();
  }
}