using VendorGateway.Application.Enums;

namespace VendorGateway.Application.Dtos
{
    public class OrderDetails
    {
        public class Order
        {
            public int Id { get; set; }
            public int AccountId { get; set; }
            public float TotalAmount { get; set; }
            public float TotalQuantity { get; set; }
            public OrderStatus Status { get; set; }
            public List<OrderItem> Items { get; set; } = [];
        }

        public class OrderItem
        {
            public int Id { get; set; }
            public int ItemId { get; set; }
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public float UnitPrice { get; set; }
        }
    }
}
