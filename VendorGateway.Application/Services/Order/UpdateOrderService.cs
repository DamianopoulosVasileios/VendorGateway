using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Enums;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using static VendorGateway.Application.Dtos.OrderRequest;

namespace VendorGateway.Application.Services.Order
{
    public class UpdateOrderService(IAccountExistenceGuard accountExistenceGuard, IProductQueries productQueries, IOrderQueries orderQueries, IOrderCommands orderCommands) : IUpdateOrderService
    {
        public async Task<Result> UpdateAsync(int accountId, int id, UpdateOrder request, CancellationToken ct)
        {
            var guard = await accountExistenceGuard.EnsureExistsAsync(accountId, ct);
            if (guard.IsFailure)
                return guard;

            var productsResult = await CheckIfProductsExist(request, ct);
            if (productsResult.IsFailure)
                return productsResult;

            var productsById = productsResult.Value;

            var orderResult = await GetUniqueOrder(accountId, id, ct);
            if (orderResult.IsFailure)
                return orderResult;

            var order = orderResult.Value;

            if (order.Status == OrderStatus.Submitted)
            {
                return Result.Failure(Error.Conflict("Cannot update an executed order."));
            }

            var productsWithCategoryIdApplicableToPotentialDiscount = productsById.Values
                .Where(p => p.Category.Equals("women's clothing", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .Distinct();

            var isCetegoryQuantityDiscountApplicable = request.Items
                .Where(x => productsWithCategoryIdApplicableToPotentialDiscount.Contains(x.ProductId))
                .Sum(x => x.Quantity) >= 5;

            foreach (var item in order.Items)
            {
                if (!productsById.TryGetValue(item.ProductId, out var product))
                    continue;

                var newItem = request.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (newItem != null)
                    item.Quantity = newItem.Quantity;

                if (isCetegoryQuantityDiscountApplicable &&
                    product.Category.Equals("jewelery", StringComparison.OrdinalIgnoreCase))
                {
                    item.UnitPrice *= 0.9f;
                }
            }

            return await orderCommands.UpdateAsync(accountId, order, ct);
        }

        private async Task<Result<Dictionary<int, Entities.Product>>> CheckIfProductsExist(UpdateOrder request, CancellationToken ct)
        {
            var products = await productQueries.GetByIdsAsync(request.Items.Select(x => x.ProductId), ct);
            var productsById = products.ToDictionary(p => p.Id);

            var missingProductIds = request.Items
                .Select(i => i.ProductId)
                .Except(productsById.Keys)
                .ToList();

            if (missingProductIds.Count > 0)
                return Result.Failure<Dictionary<int, Entities.Product>>(
                    Error.NotFound($"The following product ids were not found: {string.Join(", ", missingProductIds)}."));

            return Result.Success(productsById);
        }

        private async Task<Result<OrderDetails.Order>> GetUniqueOrder(int accountId, int id, CancellationToken ct)
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
