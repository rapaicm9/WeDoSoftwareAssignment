using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Users.UpdateUser
{
    public sealed record UpdateUserCommand(
        Guid UserId,
        string? FirstName,
        string? LastName,
        string? Email,
        string? CurrentPassword,
        string? NewPassword) : ICommand<UserResponse>;
}
