using TrainingTracker.Common.CQRS;
using TrainingTracker.Models.Enums;

namespace TrainingTracker.Features.Workouts.AddWorkout
{
    public sealed record AddWorkoutCommand(
        Guid UserId,
        string Title,
        WorkoutType WorkoutType,
        int DurationMinutes,
        int CaloriesBurned,
        int TrainingIntensity,
        int Fatigue,
        string? Notes,
        DateTime TrainingDateTimeUtc) : ICommand<AddWorkoutResponse>;
}
