using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class GetAccountService(IAccountQueries accountQueries) : IGetAccountService
    {
        public async Task<Result<Entities.Account>> GetAsync(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                return Result.Failure<Entities.Account>(Error.NotFound($"Account with id {accountId} not found."));

            return Result.Success(account.First());
        }
    }
}
