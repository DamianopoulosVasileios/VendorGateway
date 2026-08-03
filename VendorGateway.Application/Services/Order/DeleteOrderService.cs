using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;


namespace VendorGateway.Application.Services.Order
{
    public class DeleteOrderService(IAccountExistenceGuard accountExistenceGuard, IOrderCommands orderCommands) : IDeleteOrderService
    {
        public async Task<Result> DeleteAsync(int accountId, int id, CancellationToken ct)
        {
            var guard = await accountExistenceGuard.EnsureExistsAsync(accountId, ct);
            if (guard.IsFailure)
                return guard;

            return await orderCommands.DeleteByIdAsync(accountId, id, ct);
        }
    }
}
