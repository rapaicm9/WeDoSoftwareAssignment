export interface MonthlyProgressRequest {
  readonly year: number;
  readonly month: number;
}

export interface MonthlyProgressResponse {
  readonly year: number;
  readonly month: number;
  readonly weeks: readonly WeeklyProgressResponse[];
}

export interface WeeklyProgressResponse {
  readonly weekNumber: number;
  readonly weekStartDate: string;
  readonly weekEndDate: string;
  readonly totalDurationMinutes: number;
  readonly workoutCount: number;
  readonly averageTrainingIntensity: number | null;
  readonly averageFatigue: number | null;
}

export interface MonthOption {
  readonly value: number;
  readonly label: string;
}

export const MONTH_OPTIONS: readonly MonthOption[] = [
  { value: 1, label: 'January' },
  { value: 2, label: 'February' },
  { value: 3, label: 'March' },
  { value: 4, label: 'April' },
  { value: 5, label: 'May' },
  { value: 6, label: 'June' },
  { value: 7, label: 'July' },
  { value: 8, label: 'August' },
  { value: 9, label: 'September' },
  { value: 10, label: 'October' },
  { value: 11, label: 'November' },
  { value: 12, label: 'December' },
];