export type WorkoutType =
  | 'Cardio'
  | 'StrengthTraining'
  | 'Flexibility'
  | 'Bike'
  | 'Swimming';

export interface WorkoutTypeOption {
  readonly value: WorkoutType;
  readonly label: string;
}

export interface AddWorkoutRequest {
  readonly title: string;
  readonly workoutType: WorkoutType;
  readonly durationMinutes: number;
  readonly caloriesBurned: number;
  readonly trainingIntensity: number;
  readonly fatigue: number;
  readonly notes: string | null;
  readonly trainingDateTimeUtc: string;
}

export interface WorkoutResponse {
  readonly id: string;
  readonly userId: string;
  readonly title: string;
  readonly workoutType: WorkoutType;
  readonly durationMinutes: number;
  readonly caloriesBurned: number;
  readonly trainingIntensity: number;
  readonly fatigue: number;
  readonly notes: string | null;
  readonly trainingDateTimeUtc: string;
  readonly createdAtUtc: string;
}

export const WORKOUT_TYPE_OPTIONS: readonly WorkoutTypeOption[] = [
  {
    value: 'Cardio',
    label: 'Cardio',
  },
  {
    value: 'StrengthTraining',
    label: 'Strength training',
  },
  {
    value: 'Flexibility',
    label: 'Flexibility',
  },
  {
    value: 'Bike',
    label: 'Bike',
  },
  {
    value: 'Swimming',
    label: 'Swimming',
  },
];