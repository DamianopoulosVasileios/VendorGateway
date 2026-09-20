using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class UpdateAccountService(IAccountExistenceGuard accountExistenceGuard, IAccountsApiClient usersApiClient, IAccountCommands accountCommands) : IUpdateAccountService
    {
        public async Task<Result> UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct)
        {
            var guard = await accountExistenceGuard.EnsureExistsAsync(id, ct);
            if (guard.IsFailure)
                return guard;

            var response = await usersApiClient.UpdateAsync(request, id, ct);
            if (response == null || response.id == 0)
            {
                return Result.Failure(Error.Conflict("Failed to update account in the vendor system."));
            }

            return await accountCommands.UpdateAsync(id, request.email, ct);
        }
    }
}
