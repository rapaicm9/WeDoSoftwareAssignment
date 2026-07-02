using TrainingTracker.Common.CQRS;

namespace TrainingTracker.Features.Auth.Login
{
    public sealed record LoginCommand(
        string Email,
        string Password) : ICommand<LoginResponse>;
}
