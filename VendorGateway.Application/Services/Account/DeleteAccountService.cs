using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class DeleteAccountService(IAccountsApiClient usersApiClient, IAccountExistenceGuard accountExistenceGuard, IAccountCommands accountCommands) : IDeleteAccountService
    {
        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(id, ct);

            var results = await usersApiClient.DeleteAsync(id, ct);
            if (results.StatusCode != System.Net.HttpStatusCode.OK)
                throw new InvalidDataException($"Failed to delete account with id {id} from external service. Status code: {results.StatusCode}");

            await accountCommands.DeleteAsync(id, ct);
        }
    }
}
