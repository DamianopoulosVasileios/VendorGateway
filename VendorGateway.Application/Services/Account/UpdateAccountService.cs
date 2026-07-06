using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class UpdateAccountService(IAccountQueries accountQueries, IAccountsApiClient usersApiClient, IAccountCommands accountCommands) : IUpdateAccountService
    {
        public async Task UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct)
        {
            await CheckAccountExists(id, ct);

            var response = await usersApiClient.UpdateAsync(request, id, ct);
            if (response == null || response.id == 0)
            {
                throw new InvalidOperationException("Failed to update account in the vendor system.");
            }

            var results = await accountQueries.GetByIdsAsync([id], ct);
            if (results == null || results.Count == 0)
                throw new KeyNotFoundException($"Account with id {id} not found after update.");

            var account = results.SingleOrDefault();
            if (!IsUniqueAccount(account))
            {
                throw new KeyNotFoundException($"Account with id {id} is not unique.");
            }

            await accountCommands.UpdateAsync(account!, ct);
        }

        private async Task<IReadOnlyList<Entities.Account>> CheckAccountExists(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");

            return account;
        }

        private static bool IsUniqueAccount(Entities.Account? account)
        {
            return account != null;
        }
    }
}
