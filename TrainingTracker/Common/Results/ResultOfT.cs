namespace TrainingTracker.Common.Results
{
    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        private Result(T value)
            : base(true, null)
        {
            _value = value;
        }

        private Result(Error error)
            : base(false, error)
        {
            _value = default;
        }

        public T Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new InvalidOperationException("Cannot access value of a failed result.");
                }

                return _value!;
            }
        }

        public static Result<T> Success(T value)
        {
            return new Result<T>(value);
        }

        public static new Result<T> Failure(Error error)
        {
            return new Result<T>(error);
        }
    }
}
