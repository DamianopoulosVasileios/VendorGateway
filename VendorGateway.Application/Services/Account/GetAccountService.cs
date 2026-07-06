using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class GetAccountService(IAccountQueries accountQueries) : IGetAccountService
    {
        public async Task<Entities.Account?> GetAsync(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");

            return account.FirstOrDefault();
        }
    }
}
