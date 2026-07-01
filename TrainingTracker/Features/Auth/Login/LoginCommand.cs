using MediatR;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Features.Auth.Login
{
    public sealed record LoginCommand(
        string Email,
        string Password) : IRequest<Result<LoginResponse>>;
}
