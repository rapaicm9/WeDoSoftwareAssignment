using MediatR;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Common.CQRS
{
    public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>;
}
