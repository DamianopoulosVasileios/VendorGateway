using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class UpdateAccountService(IAccountExistenceGuard accountExistenceGuard, IAccountsApiClient usersApiClient, IAccountCommands accountCommands) : IUpdateAccountService
    {
        public async Task UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(id, ct);

            var response = await usersApiClient.UpdateAsync(request, id, ct);
            if (response == null || response.id == 0)
            {
                throw new InvalidOperationException("Failed to update account in the vendor system.");
            }

            await accountCommands.UpdateAsync(id, request.email, ct);
        }
    }
}
