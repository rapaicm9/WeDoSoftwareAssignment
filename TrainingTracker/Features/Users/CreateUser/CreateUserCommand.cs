using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Users.CreateUser
{
    public sealed record CreateUserCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        bool IsActive) : ICommand<UserResponse>;
}
