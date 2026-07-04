namespace VendorGateway.Infrastructure.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // NAVIGATION
        public List<OrderItem> OrderItems { get; set; } = [];
    }
}
