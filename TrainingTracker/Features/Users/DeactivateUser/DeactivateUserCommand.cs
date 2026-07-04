using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Users.DeactivateUser
{
    public sealed record DeactivateUserCommand(Guid UserId) : ICommand<UserResponse>;
}
