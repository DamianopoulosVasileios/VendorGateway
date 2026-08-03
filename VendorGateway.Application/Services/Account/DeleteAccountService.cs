using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class DeleteAccountService(IAccountsApiClient usersApiClient, IAccountExistenceGuard accountExistenceGuard, IAccountCommands accountCommands) : IDeleteAccountService
    {
        public async Task<Result> DeleteAsync(int id, CancellationToken ct)
        {
            var guard = await accountExistenceGuard.EnsureExistsAsync(id, ct);
            if (guard.IsFailure)
                return guard;

            var results = await usersApiClient.DeleteAsync(id, ct);
            if (results.StatusCode != System.Net.HttpStatusCode.OK)
                return Result.Failure(Error.Validation($"Failed to delete account with id {id} from external service. Status code: {results.StatusCode}"));

            return await accountCommands.DeleteAsync(id, ct);
        }
    }
}
