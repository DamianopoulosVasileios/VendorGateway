namespace VendorGateway.Infrastructure.Mappers
{
    public static class OrderMapper
    {
        public static Application.Dtos.OrderDetails.Order ToDto(this Application.Entities.Order order)
        {
            return new Application.Dtos.OrderDetails.Order
            {
                IdempotencyKey = order.IdempotencyKey,
                Id = order.Id,
                AccountId = order.AccountId,
                TotalAmount = order.TotalAmount,
                TotalQuantity = order.TotalQuantity,
                Status = order.Status,

                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                Items = order.Items?
                    .Select(i => i.ToDto())
                    .ToList() ?? []
            };
        }

        public static Application.Dtos.OrderDetails.OrderItem ToDto(this Application.Entities.OrderItem item)
        {
            return new Application.Dtos.OrderDetails.OrderItem
            {
                Id = item.Id,
                ItemId = item.ItemId,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };
        }

        public static Application.Entities.Order ToDto(this Application.Dtos.OrderDetails.Order order)
        {
            var items = order.Items
                .Select(i => new Application.Entities.OrderItem
                {
                    Id = i.Id,
                    ItemId = i.ItemId,
                    OrderId = i.OrderId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList() ?? [];

            var result = Application.Entities.Order.CreateWithOrderId(order.Id, order.AccountId, items);
            return result;
        }
        public static Application.Entities.OrderItem ToDto(this Application.Dtos.OrderDetails.OrderItem item)
        {
            return new Application.Entities.OrderItem
            {
                Id = item.Id,
                ItemId = item.ItemId,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };
        }
    }
}
