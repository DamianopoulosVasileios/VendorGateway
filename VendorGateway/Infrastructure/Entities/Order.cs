using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.Entities
{
    public class Order : IAuditable
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public List<OrderItem> Items { get; set; } = [];

        // NAVIGATION
        public Account Account { get; set; } = null!;
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
