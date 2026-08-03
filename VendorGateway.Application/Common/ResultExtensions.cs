namespace VendorGateway.Application.Common
{
    public static class ResultExtensions
    {
        public static Result<T> AsFailure<T>(this Result result) =>
            result.IsSuccess
                ? throw new InvalidOperationException("Cannot convert a successful Result to a failure.")
                : Result.Failure<T>(result.Error!);
    }
}
