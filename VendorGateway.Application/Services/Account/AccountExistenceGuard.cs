using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class AccountExistenceGuard(IAccountQueries accountQueries) : IAccountExistenceGuard
    {
        public async Task<Result> EnsureExistsAsync(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                return Result.Failure(Error.NotFound($"Account with id {accountId} not found."));

            return Result.Success();
        }
    }
}
