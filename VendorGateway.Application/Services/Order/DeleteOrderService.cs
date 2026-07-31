using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;


namespace VendorGateway.Application.Services.Order
{
    public class DeleteOrderService(IAccountExistenceGuard accountExistenceGuard, IOrderCommands orderCommands) : IDeleteOrderService
    {
        public async Task DeleteAsync(int accountId, int id, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(accountId, ct);

            await orderCommands.DeleteByIdAsync(accountId, id, ct);
        }
    }
}
