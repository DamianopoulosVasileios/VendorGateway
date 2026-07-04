namespace VendorGateway.Infrastructure.Entities
{
    public class Account
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;

        // NAVIGATION
        public List<Order> Orders { get; set; } = [];


    }
}
