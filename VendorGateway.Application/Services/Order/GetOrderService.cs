using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Order
{
    public class GetOrderService(IAccountQueries accountQueries, IOrderQueries orderQueries) : IGetOrderService
    {
        public async Task<OrderDetails.Order> GetAsync(int accountId, int id, CancellationToken ct)
        {
            await CheckAccountExists(accountId, ct);

            var order = await GetUniqueOrder(accountId, id, ct);
            return order;
        }

        private async Task CheckAccountExists(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");
        }
        private async Task<OrderDetails.Order> GetUniqueOrder(int accountId, int id, CancellationToken ct)
        {
            var results = await orderQueries.GetByIdsAsync(accountId, [id], ct);
            if (results == null || results.Count == 0)
            {
                throw new KeyNotFoundException($"Order with id {id} not found for account {accountId}");
            }

            var order = results.SingleOrDefault();
            if (!IsUniqueOrder(order))
            {
                throw new KeyNotFoundException($"Order with id {id} is not unique for account {accountId}");
            }

            return order!;
        }

        private static bool IsUniqueOrder(OrderDetails.Order? order)
        {
            return order != null;
        }
    }
}
