using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Enums;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Order
{
    public class CreateOrderService(IAccountExistenceGuard accountExistenceGuard, IProductQueries productQueries, IOrderQueries orderQueries, IOrderCommands orderCommands) : ICreateOrderService
    {
        public async Task<Result> CreateAsync(int accountId, Guid idempotencyKey, OrderRequest.CreateOrder request, CancellationToken ct)
        {
            var guard = await accountExistenceGuard.EnsureExistsAsync(accountId, ct);
            if (guard.IsFailure)
                return guard;

            var productsResult = await CheckIfRequestProductsExist(request, ct);
            if (productsResult.IsFailure)
                return productsResult;

            var productByIds = productsResult.Value;

            var pendingOrderCheck = await CheckIfOrderProductExistsInAnotherOrderWithStatusPending(accountId, productByIds, ct);
            if (pendingOrderCheck.IsFailure)
                return pendingOrderCheck;

            var productsWithCategoryIdApplicableToPotentialDiscount = productByIds.Values
                .Where(p => p.Category.Equals("women's clothing", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .Distinct();

            var isCetegoryQuantityDiscountApplicable = request.Items
                .Where(x => productsWithCategoryIdApplicableToPotentialDiscount.Contains(x.ProductId))
                .Sum(x => x.Quantity) >= 5;

            var orderItems = request.Items
                .Select((item, index) =>
                {
                    var product = productByIds[item.ProductId];

                    var unitPrice = product.Price;

                    if (isCetegoryQuantityDiscountApplicable &&
                        product.Category.Equals("jewelery", StringComparison.OrdinalIgnoreCase))
                    {
                        unitPrice *= 0.9f;
                    }

                    return new OrderDetails.OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        ItemId = index + 1
                    };
                })
                .ToList();

            return await orderCommands.CreateAsync(accountId, idempotencyKey, orderItems, ct);
        }

        private async Task<Result<Dictionary<int, Entities.Product>>> CheckIfRequestProductsExist(OrderRequest.CreateOrder request, CancellationToken ct)
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

        private async Task<Result> CheckIfOrderProductExistsInAnotherOrderWithStatusPending(int accountId, Dictionary<int, Entities.Product> productByIds, CancellationToken ct)
        {
            var orders = await orderQueries.GetAsync(accountId, ct);

            var existingOrder = orders.Any(order =>
                order.Status == OrderStatus.Pending &&
                order.Items.Any(item => productByIds.ContainsKey(item.ProductId)));

            if (existingOrder)
                return Result.Failure(Error.Conflict("Cannot create a new order if you already have a pending order with at least one of the given product ids"));

            return Result.Success();
        }
    }
}
