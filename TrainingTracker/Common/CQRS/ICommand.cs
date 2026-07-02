using MediatR;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Common.CQRS
{
    public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
}
