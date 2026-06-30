using MediatR;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Features.Auth.Register
{
    public sealed record RegisterCommand(
        string Email,
        string Password,
        string FirstName,
        string LastName
    ) : IRequest<Result<RegisterResponse>>;
}
