using MediatR;
using TrainingTracker.Common.Results;


namespace TrainingTracker.Features.Workouts.GetMonthlyProgress
{
    public sealed record GetMonthlyProgressQuery(
        Guid UserId,
        int Year,
        int Month) : IRequest<Result<GetMonthlyProgressResponse>>;
}
