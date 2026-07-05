import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { TokenStorageService } from '../../../../core/auth/token-storage.service';
import { WorkoutResponse } from '../../../workouts/data-access/workout.models';
import { WorkoutsApiService } from '../../../workouts/data-access/workouts-api.service';

interface DashboardFeatureCard {
  readonly title: string;
  readonly description: string;
  readonly route: string;
  readonly icon: string;
  readonly actionLabel: string;
}

@Component({
  selector: 'app-dashboard-home-page',
  imports: [
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatSnackBarModule,
  ],
  templateUrl: './dashboard-home-page.component.html',
  styleUrl: './dashboard-home-page.component.scss',
})
export class DashboardHomePageComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly tokenStorageService = inject(TokenStorageService);
  private readonly workoutsApiService = inject(WorkoutsApiService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly workouts = signal<readonly WorkoutResponse[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly hasLoadError = signal(false);

  protected readonly currentMonthName = new Intl.DateTimeFormat('en-US', {
    month: 'long',
  }).format(new Date());

  protected readonly heroTitle = computed(() => {
    const firstName = this.tokenStorageService.currentUser()?.firstName?.trim();

    return firstName ? `Welcome back, ${firstName}` : 'Welcome back';
  });

  protected readonly currentMonthWorkouts = computed(() => {
    return this.workouts().filter((workout) =>
      this.isWorkoutInCurrentMonth(workout.trainingDateTimeUtc),
    );
  });

  protected readonly workoutsThisMonthCount = computed(() => {
    return this.currentMonthWorkouts().length;
  });

  protected readonly totalDurationThisMonth = computed(() => {
    return this.currentMonthWorkouts().reduce(
      (total, workout) => total + workout.durationMinutes,
      0,
    );
  });

  protected readonly latestWorkout = computed(() => {
    return [...this.workouts()].sort((firstWorkout, secondWorkout) => {
      return (
        this.getDateTimeValue(secondWorkout.trainingDateTimeUtc) -
        this.getDateTimeValue(firstWorkout.trainingDateTimeUtc)
      );
    })[0] ?? null;
  });

  protected readonly featureCards: DashboardFeatureCard[] = [
    {
      title: 'Workouts',
      description: 'Log new workouts and review your training history.',
      route: '/dashboard/workouts',
      icon: 'fitness_center',
      actionLabel: 'Open workouts',
    },
    {
      title: 'Monthly Progress',
      description: 'Review weekly totals, averages, and training consistency.',
      route: '/dashboard/progress',
      icon: 'monitoring',
      actionLabel: 'View progress',
    },
    {
      title: 'Profile',
      description: 'View your account information and profile details.',
      route: '/dashboard/profile',
      icon: 'account_circle',
      actionLabel: 'Open profile',
    },
  ];

  public ngOnInit(): void {
    this.loadWorkouts();
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

  protected formatWorkoutType(workoutType: string): string {
    return workoutType.replace(/([a-z])([A-Z])/g, '$1 $2');
  }

  protected formatWorkoutDate(value: string): string {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return value;
    }

    return new Intl.DateTimeFormat('en-US', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    }).format(date);
  }

  private loadWorkouts(): void {
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
          this.workouts.set(response);
        },
        error: () => {
          this.workouts.set([]);
          this.hasLoadError.set(true);

          this.snackBar.open(
            'Failed to load dashboard summary. Please try again.',
            'Close',
            {
              duration: 4000,
            },
          );
        },
      });
  }

  private isWorkoutInCurrentMonth(value: string): boolean {
    const workoutDate = new Date(value);

    if (Number.isNaN(workoutDate.getTime())) {
      return false;
    }

    const now = new Date();

    return (
      workoutDate.getFullYear() === now.getFullYear() &&
      workoutDate.getMonth() === now.getMonth()
    );
  }

  private getDateTimeValue(value: string): number {
    const date = new Date(value);

    if (Number.isNaN(date.getTime())) {
      return 0;
    }

    return date.getTime();
  }
}