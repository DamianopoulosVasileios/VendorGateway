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
    }
}
