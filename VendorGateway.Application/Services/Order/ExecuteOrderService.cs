using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Order
{
    public class ExecuteOrderService(IAccountExistenceGuard accountExistenceGuard, IOrderQueries orderQueries, IOrderCommands orderCommands) : IExecuteOrderService
    {
        public async Task<Result> ExecuteAsync(int accountId, int id, CancellationToken ct)
        {
            var guard = await accountExistenceGuard.EnsureExistsAsync(accountId, ct);
            if (guard.IsFailure)
                return guard;

            var orderResult = await OrderExistsAndIsUnique(accountId, id, ct);
            if (orderResult.IsFailure)
                return orderResult;

            var order = orderResult.Value;

            return await orderCommands.ExecuteAsync(order.AccountId, order.Id, ct);
        }

        private async Task<Result<OrderDetails.Order>> OrderExistsAndIsUnique(int accountId, int id, CancellationToken ct)
        {
            var results = await orderQueries.GetByIdsAsync(accountId, [id], ct);
            if (results == null || results.Count == 0)
            {
                return Result.Failure<OrderDetails.Order>(Error.NotFound($"Order with id {id} not found for account {accountId}"));
            }

            var order = results.SingleOrDefault();
            return Result.Success(order!);
        }
    }
}
