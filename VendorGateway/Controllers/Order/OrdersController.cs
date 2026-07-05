using Microsoft.AspNetCore.Mvc;
using VendorGateway.Application.Dtos;
using VendorGateway.Contracts.Order.Requests;
using VendorGateway.Enums;
using VendorGateway.Infrastructure.Entities;
using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Controllers.Order
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController(
        IAccountQueries accountQueries,
        IProductQueries productQueries,
        IOrderQueries orderQueries,
        IOrderCommands orderCommands
        ) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int accountId, int id, CancellationToken ct)
        {
            await CheckAccountExists(accountId, ct);

            var order = await OrderExistsAndIsUnique(accountId, id, ct);
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(ApiCreateOrderRequest request, CancellationToken ct)
        {
            await CheckAccountExists(request.AccountId, ct);

            var productByIds = await CheckIfRequestProductsExist(request, ct);

            await CheckIfOrderProductExistsInAnotherOrderWithStatusPending(request, productByIds, ct);

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

                    return new Application.Dtos.OrderDetails.OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        ItemId = index + 1
                    };
                })
                .ToList();

            await orderCommands.CreateAsync(request.AccountId, orderItems, ct);

            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrder(int accountId, int id, ApiUpdateOrderRequest request, CancellationToken ct)
        {
            await CheckAccountExists(accountId, ct);

            var productsById = await CheckIfProductsExist(request, ct);
            var order = await OrderExistsAndIsUnique(accountId, id, ct);

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

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteByIdOrder(int accountId, int id, CancellationToken ct)
        {
            await CheckAccountExists(accountId, ct);

            await orderCommands.DeleteByIdAsync(accountId, id, ct);
            return Ok();
        }

        [HttpPost("execute/{id:int}")]
        public async Task<IActionResult> ExecuteOrder(int accountId, int id, CancellationToken ct)
        {
            await CheckAccountExists(accountId, ct);

            var order = await OrderExistsAndIsUnique(accountId, id, ct);

            await orderCommands.ExecuteAsync(order.AccountId, order.Id, ct);

            return Ok();
        }

        //[HttpGet]
        //public async Task<IActionResult> GetOrders(int accountId, CancellationToken ct)
        //{
        //    await CheckAccountExists(accountId, ct);

        //    var results = await orderQueries.GetAsync(accountId, ct);
        //    return Ok(results);
        //}

        //[HttpDelete]
        //public async Task<IActionResult> DeleteOrders(int accountId, CancellationToken ct)
        //{
        //    await CheckAccountExists(accountId, ct);

        //    await orderCommands.DeleteAsync(accountId, ct);
        //    return Ok();
        //}
        //[HttpGet("orderItems")]
        //public async Task<IActionResult> GetOrderItems(int accountId, CancellationToken ct)
        //{
        //    await CheckAccountExists(accountId, ct);

        //    var orders = await orderQueries.GetAsync(accountId, ct);
        //    var orderIds = orders.Select(x => x.Id);

        //    var results = await orderQueries.GetOrderItemsAsync(orderIds, ct);
        //    return Ok(results);
        //}

        private async Task CheckIfOrderProductExistsInAnotherOrderWithStatusPending(ApiCreateOrderRequest request, Dictionary<int, Product> productByIds, CancellationToken ct)
        {
            var orders = await orderQueries.GetAsync(request.AccountId, ct);

            var existingOrder = orders.Any(order =>
                order.Status == OrderStatus.Pending &&
                order.Items.Any(item => productByIds.ContainsKey(item.ProductId)));

            if (existingOrder)
                throw new InvalidCastException("Cannot create a new order if you already have a pending order with at least one of the given product ids");
        }
        private async Task CheckAccountExists(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");
        }
        private async Task<Dictionary<int, Product>> CheckIfRequestProductsExist(ApiCreateOrderRequest request, CancellationToken ct)
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
        private async Task<Dictionary<int, Product>> CheckIfRequestProductsExist(ApiUpdateOrderRequest request, CancellationToken ct)
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
        private async Task<Dictionary<int, Product>> CheckIfProductsExist(ApiUpdateOrderRequest request, CancellationToken ct)
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

        private async Task<OrderDetails.Order> OrderExistsAndIsUnique(int accountId, int id, CancellationToken ct)
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
