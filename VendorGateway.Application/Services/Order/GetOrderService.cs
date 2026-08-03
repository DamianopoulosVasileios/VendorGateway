using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Order
{
    public class GetOrderService(IAccountExistenceGuard accountExistenceGuard, IOrderQueries orderQueries) : IGetOrderService
    {
        public async Task<Result<OrderDetails.Order>> GetAsync(int accountId, int id, CancellationToken ct)
        {
            var guard = await accountExistenceGuard.EnsureExistsAsync(accountId, ct);
            if (guard.IsFailure)
                return guard.AsFailure<OrderDetails.Order>();

            return await GetUniqueOrder(accountId, id, ct);
        }

        private async Task<Result<OrderDetails.Order>> GetUniqueOrder(int accountId, int id, CancellationToken ct)
        {
            var results = await orderQueries.GetByIdsAsync(accountId, [id], ct);
            if (results == null || results.Count == 0)
            {
                return Result.Failure<OrderDetails.Order>(Error.NotFound($"Order with id {id} not found for account {accountId}"));
            }

            var order = results.SingleOrDefault();
            if (!IsUniqueOrder(order))
            {
                return Result.Failure<OrderDetails.Order>(Error.NotFound($"Order with id {id} is not unique for account {accountId}"));
            }

            return Result.Success(order!);
        }

        private static bool IsUniqueOrder(OrderDetails.Order? order)
        {
            return order != null;
        }
    }
}
