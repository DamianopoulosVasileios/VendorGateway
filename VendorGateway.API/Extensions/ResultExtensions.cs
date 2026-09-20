using Microsoft.AspNetCore.Mvc;
using VendorGateway.Application.Common;

namespace VendorGateway.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result) =>
            result.IsSuccess ? new OkResult() : ToProblem(result.Error!);

        public static IActionResult ToActionResult<T>(this Result<T> result) =>
            result.IsSuccess ? new OkObjectResult(result.Value) : ToProblem(result.Error!);

        public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, object> map) =>
            result.IsSuccess ? new OkObjectResult(map(result.Value)) : ToProblem(result.Error!);

        private static IActionResult ToProblem(Error error)
        {
            var statusCode = error.Category switch
            {
                ErrorCategory.NotFound => StatusCodes.Status404NotFound,
                ErrorCategory.Validation => StatusCodes.Status400BadRequest,
                ErrorCategory.Conflict => StatusCodes.Status409Conflict,
                ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            var message = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : error.Message;

            return new ObjectResult(new { message }) { StatusCode = statusCode };
        }
    }
}
