using VendorGateway.Application.Enums;
using VendorGateway.Application.Interfaces;

namespace VendorGateway.Application.Entities
{
    public class Order : IAuditable
    {
        public static Order Create(int accountId, List<OrderItem> items)
        {
            return new Order(accountId, items);
        }
        public static Order CreateWithOrderId(int id, int accountId, List<OrderItem> items)
        {
            return new Order(id, accountId, items);
        }

        public int Id { get; private set; }
        public int AccountId { get; private set; }
        public float TotalAmount { get; private set; }
        public float TotalQuantity { get; private set; }
        public OrderStatus Status { get; private set; }

        // Navigation
        public Account Account { get; private set; } = null!;
        // Navigation
        public List<OrderItem> Items { get; private set; } = new();

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        private Order() { }

        private Order(int accountId, List<OrderItem> items)
        {
            AccountId = accountId;
            Status = OrderStatus.Pending;
            Items = items;

            CalculateTotalAmount();
            CalculateTotalQuantity();
        }
        private Order(int id, int accountId, List<OrderItem> items)
        {
            Id = id;
            AccountId = accountId;
            Status = OrderStatus.Pending;
            Items = items;

            CalculateTotalAmount();
            CalculateTotalQuantity();
        }

        public void CalculateTotalAmount()
        {
            TotalAmount = Items.Sum(item => item.UnitPrice * item.Quantity);
        }

        public void CalculateTotalQuantity()
        {
            TotalQuantity = Items.Sum(item => item.Quantity);
        }

        public void UpdatePropertiesForOrderUpdate(Order order)
        {
            Items = [.. order.Items];
            TotalAmount = order.TotalAmount;
            TotalQuantity = order.TotalQuantity;
        }
        public void ExecuteOrder()
        {
            Status = OrderStatus.Submitted;
        }
    }
}
