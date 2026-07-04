using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Workouts.DeleteWorkout
{
    public sealed record DeleteWorkoutCommand(
        Guid UserId,
        Guid WorkoutId) : ICommand<DeleteWorkoutResponse>;
}
