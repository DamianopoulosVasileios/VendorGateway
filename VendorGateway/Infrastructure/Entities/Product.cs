using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.Entities
{
    public class Product : IAuditable
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public float Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
