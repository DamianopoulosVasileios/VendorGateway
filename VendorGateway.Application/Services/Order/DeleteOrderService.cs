using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;


namespace VendorGateway.Application.Services.Order
{
    public class DeleteOrderService(IAccountQueries accountQueries, IOrderCommands orderCommands) : IDeleteOrderService
    {
        public async Task DeleteAsync(int accountId, int id, CancellationToken ct)
        {
            await CheckAccountExists(accountId, ct);

            await orderCommands.DeleteByIdAsync(accountId, id, ct);
        }

        private async Task CheckAccountExists(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");
        }
    }
}
