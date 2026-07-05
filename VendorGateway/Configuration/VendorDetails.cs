namespace VendorGateway.Configuration
{
    public class VendorSettings
    {
        public List<VendorDetails> VendorDetails { get; set; } = [];
    }

    public class VendorDetails
    {
        public string Name { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; }

        public ProductsEndpoints? Products { get; set; } = null;
        public UsersEndpoints? Users { get; set; } = null;
    }

    public class ProductsEndpoints
    {
        public string GetAll { get; set; } = string.Empty;
        public string Delete { get; set; } = string.Empty;
    }

    public class UsersEndpoints
    {
        public string Create { get; set; } = string.Empty;
        public string Get { get; set; } = string.Empty;
        public string Update { get; set; } = string.Empty;
        public string Delete { get; set; } = string.Empty;

    }
}
