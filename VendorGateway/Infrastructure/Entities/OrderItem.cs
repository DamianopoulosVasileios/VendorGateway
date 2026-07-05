using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.Entities
{
    public class OrderItem : IAuditable
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public float UnitPrice { get; set; }

        // NAVIGATION
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
