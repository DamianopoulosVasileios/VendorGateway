namespace VendorGateway.Infrastructure.Mappers
{
    public static class OrderMapper
    {
        public static Application.Dtos.OrderDetails.Order ToDto(this Entities.Order order)
        {
            return new Application.Dtos.OrderDetails.Order
            {
                Id = order.Id,
                AccountId = order.AccountId,
                TotalAmount = order.TotalAmount,
                TotalQuantity = order.TotalQuantity,
                Status = order.Status,

                Items = order.Items?
                    .Select(i => i.ToDto())
                    .ToList() ?? []
            };
        }

        public static Application.Dtos.OrderDetails.OrderItem ToDto(this Entities.OrderItem item)
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

        public static Entities.Order ToDto(this Application.Dtos.OrderDetails.Order order)
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
        public static Entities.OrderItem ToDto(this Application.Dtos.OrderDetails.OrderItem item)
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
