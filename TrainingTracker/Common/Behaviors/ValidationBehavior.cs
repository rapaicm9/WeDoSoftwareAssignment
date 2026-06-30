using System.Reflection;
using FluentValidation;
using MediatR;
using TrainingTracker.Common.Results;

namespace TrainingTracker.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

        public ValidationBehavior(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidationBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next(cancellationToken);
            }

            var validationContext = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(validator =>
                    validator.ValidateAsync(validationContext, cancellationToken)));

            var validationFailures = validationResults
                .SelectMany(result => result.Errors)
                .Where(error => error is not null)
                .ToList();

            if (validationFailures.Count == 0)
            {
                return await next(cancellationToken);
            }

            var errorMessage = string.Join(
                "; ",
                validationFailures.Select(failure =>
                    $"{failure.PropertyName}: {failure.ErrorMessage}"));

            _logger.LogWarning(
                "Validation failed for request {RequestName}: {ValidationErrors}",
                typeof(TRequest).Name,
                errorMessage);

            var error = new Error(
                Code: "Validation.Failed",
                Message: errorMessage,
                Type: ErrorType.Validation);

            return CreateValidationResult<TResponse>(error);
        }

        private static TResult CreateValidationResult<TResult>(Error error)
        {
            if (typeof(TResult) == typeof(Result))
            {
                return (TResult)(object)Result.Failure(error);
            }

            if (typeof(TResult).IsGenericType &&
                typeof(TResult).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];

                var failureMethod = typeof(Result<>)
                    .MakeGenericType(resultType)
                    .GetMethod(
                        nameof(Result<object>.Failure),
                        BindingFlags.Public | BindingFlags.Static);

                if (failureMethod is null)
                {
                    throw new InvalidOperationException(
                        "Could not find Result<T>.Failure method.");
                }

                return (TResult)failureMethod.Invoke(null, new object[] { error })!;
            }

            throw new InvalidOperationException(
                "ValidationBehavior only supports Result or Result<T> response types.");
        }
    }
}
