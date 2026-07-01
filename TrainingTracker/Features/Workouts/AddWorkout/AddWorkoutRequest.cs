using TrainingTracker.Models.Enums;

namespace TrainingTracker.Features.Workouts.AddWorkout
{
    public sealed record AddWorkoutRequest(
        string Title,
        WorkoutType WorkoutType,
        int DurationMinutes,
        int CaloriesBurned,
        int TrainingIntensity,
        int Fatigue,
        string? Notes,
        DateTime TrainingDateTimeUtc);
}
