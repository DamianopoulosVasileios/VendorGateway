using VendorGateway.Application.Dtos;
using VendorGateway.Application.Enums;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Order
{
    public class CreateOrderService(IAccountExistenceGuard accountExistenceGuard, IProductQueries productQueries, IOrderQueries orderQueries, IOrderCommands orderCommands) : ICreateOrderService
    {
        public async Task CreateAsync(int accountId, Guid idempotencyKey, OrderRequest.CreateOrder request, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(accountId, ct);

            var productByIds = await CheckIfRequestProductsExist(request, ct);

            await CheckIfOrderProductExistsInAnotherOrderWithStatusPending(accountId, productByIds, ct);

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
                    if (!productByIds.TryGetValue(item.ProductId, out var product))
                        throw new KeyNotFoundException($"Product {item.ProductId} not found");

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

            await orderCommands.CreateAsync(accountId, idempotencyKey, orderItems, ct);
        }

        private async Task<Dictionary<int, Entities.Product>> CheckIfRequestProductsExist(OrderRequest.CreateOrder request, CancellationToken ct)
        {
            var products = await productQueries.GetByIdsAsync(request.Items.Select(x => x.ProductId), ct);
            var productsById = products.ToDictionary(p => p.Id);

            var missingProductIds = request.Items
                .Select(i => i.ProductId)
                .Except(productsById.Keys)
                .ToList();

            if (missingProductIds.Count > 0)
                throw new KeyNotFoundException($"The following product ids were not found: {string.Join(", ", missingProductIds)}.");

            return productsById;
        }

        private async Task CheckIfOrderProductExistsInAnotherOrderWithStatusPending(int accountId, Dictionary<int, Entities.Product> productByIds, CancellationToken ct)
        {
            var orders = await orderQueries.GetAsync(accountId, ct);

            var existingOrder = orders.Any(order =>
                order.Status == OrderStatus.Pending &&
                order.Items.Any(item => productByIds.ContainsKey(item.ProductId)));

            if (existingOrder)
                throw new InvalidOperationException("Cannot create a new order if you already have a pending order with at least one of the given product ids");
        }
    }
}
