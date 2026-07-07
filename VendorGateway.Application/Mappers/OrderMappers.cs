namespace VendorGateway.Application.Mappers
{
    public static class OrderMappers
    {
        public static Dtos.OrderDetails.Order ToDto(this Entities.Order order)
        {
            return new Dtos.OrderDetails.Order
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

        public static Dtos.OrderDetails.OrderItem ToDto(this Entities.OrderItem item)
        {
            return new Dtos.OrderDetails.OrderItem
            {
                Id = item.Id,
                ItemId = item.ItemId,
                OrderId = item.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            };
        }

        public static Entities.Order ToDto(this Dtos.OrderDetails.Order order)
        {
            var items = order.Items
                .Select(i => new Entities.OrderItem
                {
                    Id = i.Id,
                    ItemId = i.ItemId,
                    OrderId = i.OrderId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList() ?? [];

            var result = Entities.Order.CreateWithOrderId(order.Id, order.AccountId, items);
            return result;
        }
        public static Entities.OrderItem ToDto(this Dtos.OrderDetails.OrderItem item)
        {
            return new Entities.OrderItem
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
