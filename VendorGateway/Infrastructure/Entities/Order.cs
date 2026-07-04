namespace VendorGateway.Infrastructure.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public List<OrderItem> Items { get; set; } = [];

        // NAVIGATION
        public Account Account { get; set; } = null!;
    }
}
