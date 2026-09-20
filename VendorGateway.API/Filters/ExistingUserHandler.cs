using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.API.Filters
{
    public class ExistingUserRequirement : IAuthorizationRequirement
    {
    }
    public class ExistingUserHandler(IAuthorizationQueries authorizationQueries) : AuthorizationHandler<ExistingUserRequirement>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ExistingUserRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || !int.TryParse(userId, out var id))
                return;

            var user = await authorizationQueries.GetUserByIdAsync(id);

            if (user != null)
            {
                context.Succeed(requirement);
            }
        }
    }
}
