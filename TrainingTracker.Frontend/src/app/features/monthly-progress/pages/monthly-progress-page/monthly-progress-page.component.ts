import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { finalize } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';

import {
  MONTH_OPTIONS,
  MonthlyProgressResponse,
  WeeklyProgressResponse,
} from '../../data-access/monthly-progress.models';
import { MonthlyProgressApiService } from '../../data-access/monthly-progress-api.service';

@Component({
  selector: 'app-monthly-progress-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTableModule,
  ],
  templateUrl: './monthly-progress-page.component.html',
  styleUrl: './monthly-progress-page.component.scss',
})
export class MonthlyProgressPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);
  private readonly monthlyProgressApiService = inject(MonthlyProgressApiService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly monthOptions = MONTH_OPTIONS;
  protected readonly isLoading = signal(false);
  protected readonly monthlyProgress = signal<MonthlyProgressResponse | null>(null);

  protected readonly displayedColumns: readonly string[] = [
    'weekNumber',
    'dateRange',
    'workoutCount',
    'totalDurationMinutes',
    'averageTrainingIntensity',
    'averageFatigue',
  ];

  protected readonly form = this.formBuilder.nonNullable.group({
    year: [
      new Date().getFullYear(),
      [Validators.required, Validators.min(1), Validators.max(9999)],
    ],
    month: [
      new Date().getMonth() + 1,
      [Validators.required, Validators.min(1), Validators.max(12)],
    ],
  });

  protected readonly weeks = computed(() => {
    return this.monthlyProgress()?.weeks ?? [];
  });

  protected readonly totalWorkoutCount = computed(() => {
    return this.weeks().reduce(
      (total, week) => total + week.workoutCount,
      0,
    );
  });

  protected readonly totalDurationMinutes = computed(() => {
    return this.weeks().reduce(
      (total, week) => total + week.totalDurationMinutes,
      0,
    );
  });

  protected readonly averageTrainingIntensity = computed(() => {
    return this.calculateWeightedAverage(
      this.weeks(),
      (week) => week.averageTrainingIntensity,
    );
  });

  protected readonly averageFatigue = computed(() => {
    return this.calculateWeightedAverage(
      this.weeks(),
      (week) => week.averageFatigue,
    );
  });

  public ngOnInit(): void {
    this.loadMonthlyProgress();
  }

  protected loadMonthlyProgress(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();

      this.snackBar.open('Please select a valid month and year.', 'Close', {
        duration: 3000,
      });

      return;
    }

    const request = this.form.getRawValue();

    this.isLoading.set(true);

    this.monthlyProgressApiService
      .getMonthlyProgress(request)
      .pipe(
        finalize(() => {
          this.isLoading.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (response) => {
          this.monthlyProgress.set(response);
        },
        error: () => {
          this.monthlyProgress.set(null);

          this.snackBar.open(
            'Failed to load monthly progress. Please try again.',
            'Close',
            {
              duration: 4000,
            },
          );
        },
      });
  }

  protected formatDateRange(week: WeeklyProgressResponse): string {
    return `${this.formatDateOnly(week.weekStartDate)} - ${this.formatDateOnly(
      week.weekEndDate,
    )}`;
  }

  protected formatNullableAverage(value: number | null): string {
    if (value === null) {
      return '-';
    }

    return value.toFixed(2);
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

  private calculateWeightedAverage(
    weeks: readonly WeeklyProgressResponse[],
    selector: (week: WeeklyProgressResponse) => number | null,
  ): number | null {
    const weeksWithWorkouts = weeks.filter(
      (week) => week.workoutCount > 0 && selector(week) !== null,
    );

    const totalWorkoutCount = weeksWithWorkouts.reduce(
      (total, week) => total + week.workoutCount,
      0,
    );

    if (totalWorkoutCount === 0) {
      return null;
    }

    const weightedTotal = weeksWithWorkouts.reduce((total, week) => {
      const value = selector(week);

      if (value === null) {
        return total;
      }

      return total + value * week.workoutCount;
    }, 0);

    return weightedTotal / totalWorkoutCount;
  }

  private formatDateOnly(value: string): string {
    const [year, month, day] = value.split('-');

    if (!year || !month || !day) {
      return value;
    }

    return `${day}.${month}.${year}`;
  }
}