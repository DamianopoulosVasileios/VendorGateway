namespace VendorGateway.Application.Common
{
    public enum ErrorCategory
    {
        NotFound,
        Conflict,
        Validation,
        Unauthorized,
        Unexpected
    }

    public sealed record Error(ErrorCategory Category, string Message)
    {
        public static Error NotFound(string message) => new(ErrorCategory.NotFound, message);
        public static Error Conflict(string message) => new(ErrorCategory.Conflict, message);
        public static Error Validation(string message) => new(ErrorCategory.Validation, message);
        public static Error Unauthorized(string message) => new(ErrorCategory.Unauthorized, message);
        public static Error Unexpected(string message) => new(ErrorCategory.Unexpected, message);
    }
}
