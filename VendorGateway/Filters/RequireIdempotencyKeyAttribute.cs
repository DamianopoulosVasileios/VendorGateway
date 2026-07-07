using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VendorGateway.API.Filters
{
    public sealed class RequireIdempotencyKeyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var value)
                || !Guid.TryParse(value, out var key)
                || key == Guid.Empty)
            {
                context.Result = new ObjectResult(new ProblemDetails
                {
                    Title = "Missing or invalid Idempotency-Key header.",
                    Detail = "A valid, non-empty GUID must be supplied in the 'Idempotency-Key' header.",
                    Status = StatusCodes.Status400BadRequest
                })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                return;
            }

            context.HttpContext.Items["IdempotencyKey"] = key;
        }
    }
}
