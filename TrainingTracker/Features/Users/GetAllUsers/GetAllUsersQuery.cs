using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Users.GetAllUsers
{
    public sealed record GetAllUsersQuery()
        : IQuery<IReadOnlyCollection<UserResponse>>;
}
