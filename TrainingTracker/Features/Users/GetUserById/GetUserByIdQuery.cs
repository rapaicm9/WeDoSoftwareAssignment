using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Users.GetUserById
{
    public sealed record GetUserByIdQuery(Guid UserId)
        : IQuery<UserResponse>;
}
