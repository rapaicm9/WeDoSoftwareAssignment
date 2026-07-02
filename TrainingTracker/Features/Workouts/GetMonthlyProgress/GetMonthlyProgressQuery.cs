using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Workouts.GetMonthlyProgress
{
    public sealed record GetMonthlyProgressQuery(
        Guid UserId,
        int Year,
        int Month) : IQuery<GetMonthlyProgressResponse>;
}
