using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Order
{
    public class ExecuteOrderService(IAccountExistenceGuard accountExistenceGuard, IOrderQueries orderQueries, IOrderCommands orderCommands) : IExecuteOrderService
    {
        public async Task ExecuteAsync(int accountId, int id, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(accountId, ct);

            var order = await OrderExistsAndIsUnique(accountId, id, ct);

            await orderCommands.ExecuteAsync(order.AccountId, order.Id, ct);
        }

        private async Task<OrderDetails.Order> OrderExistsAndIsUnique(int accountId, int id, CancellationToken ct)
        {
            var results = await orderQueries.GetByIdsAsync(accountId, [id], ct);
            if (results == null || results.Count == 0)
            {
                throw new KeyNotFoundException($"Order with id {id} not found for account {accountId}");
            }

            var order = results.SingleOrDefault();
            return order!;
        }
    }
}
