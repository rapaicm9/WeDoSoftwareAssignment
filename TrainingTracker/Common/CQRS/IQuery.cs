using MediatR;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Common.CQRS
{
    public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
}
