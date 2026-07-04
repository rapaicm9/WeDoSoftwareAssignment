using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Workouts.GetWorkouts
{
    public sealed record GetWorkoutsQuery(Guid UserId)
        : IQuery<IReadOnlyCollection<WorkoutResponse>>;
}
