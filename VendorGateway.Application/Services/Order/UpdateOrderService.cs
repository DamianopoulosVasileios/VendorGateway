using VendorGateway.Application.Dtos;
using VendorGateway.Application.Enums;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using static VendorGateway.Application.Dtos.OrderRequest;

namespace VendorGateway.Application.Services.Order
{
    public class UpdateOrderService(IAccountQueries accountQueries, IProductQueries productQueries, IOrderQueries orderQueries, IOrderCommands orderCommands) : IUpdateOrderService
    {
        public async Task UpdateAsync(int accountId, int id, UpdateOrder request, CancellationToken ct)
        {
            await CheckAccountExists(accountId, ct);

            var productsById = await CheckIfProductsExist(request, ct);
            var order = await GetUniqueOrder(accountId, id, ct);

            if (order.Status == OrderStatus.Submitted)
            {
                throw new InvalidOperationException("Cannot update an executed order.");
            }

            var productByIds = await CheckIfRequestProductsExist(request, ct);

            var productsWithCategoryIdApplicableToPotentialDiscount = productByIds.Values
                .Where(p => p.Category.Equals("women's clothing", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Id)
                .Distinct();

            var isCetegoryQuantityDiscountApplicable = request.Items
                .Where(x => productsWithCategoryIdApplicableToPotentialDiscount.Contains(x.ProductId))
                .Sum(x => x.Quantity) >= 5;

            foreach (var item in order.Items)
            {
                if (!productByIds.TryGetValue(item.ProductId, out var product))
                    throw new KeyNotFoundException($"Product {item.ProductId} not found");

                var newItem = request.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (newItem != null)
                    item.Quantity = newItem.Quantity;

                if (isCetegoryQuantityDiscountApplicable &&
                    product.Category.Equals("jewelery", StringComparison.OrdinalIgnoreCase))
                {
                    item.UnitPrice *= 0.9f;
                }
            }

            await orderCommands.UpdateAsync(request.AccountId, order, ct);
        }


        private async Task CheckAccountExists(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");
        }

        private async Task<Dictionary<int, Entities.Product>> CheckIfRequestProductsExist(UpdateOrder request, CancellationToken ct)
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

        private async Task<Dictionary<int, Entities.Product>> CheckIfProductsExist(UpdateOrder request, CancellationToken ct)
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
