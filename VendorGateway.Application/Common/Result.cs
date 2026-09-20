namespace VendorGateway.Application.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error? Error { get; }

        protected Result(bool isSuccess, Error? error)
        {
            if (isSuccess && error is not null)
                throw new InvalidOperationException("A successful Result cannot carry an Error.");
            if (!isSuccess && error is null)
                throw new InvalidOperationException("A failed Result must carry an Error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(Error error) => new(false, error);
        public static Result<T> Success<T>(T value) => new(value, true, null);
        public static Result<T> Failure<T>(Error error) => new(default, false, error);
    }

    public sealed class Result<T> : Result
    {
        private readonly T? _value;

        public T Value => IsSuccess
            ? _value!
            : throw new InvalidOperationException("Cannot read the Value of a failed Result.");

        internal Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
        {
            _value = value;
        }
    }
}
