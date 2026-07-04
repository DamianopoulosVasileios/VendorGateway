using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.Entities
{
    public class Account : IAuditable
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        // NAVIGATION
        public List<Order> Orders { get; set; } = [];

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
