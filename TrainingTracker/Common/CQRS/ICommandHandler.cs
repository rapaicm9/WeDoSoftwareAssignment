using MediatR;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Common.CQRS
{
    public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>> where TCommand : ICommand<TResponse>;
}
