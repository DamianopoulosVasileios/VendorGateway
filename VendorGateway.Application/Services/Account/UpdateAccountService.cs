using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class UpdateAccountService(IAccountQueries accountQueries, IAccountsApiClient usersApiClient) : IUpdateAccountService
    {
        public async Task UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct)
        {
            await CheckAccountExists(id, ct);
            var results = await usersApiClient.UpdateAsync(request, id, ct);
        }

        private async Task<IReadOnlyList<Entities.Account>> CheckAccountExists(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");

            return account;
        }
    }
}
