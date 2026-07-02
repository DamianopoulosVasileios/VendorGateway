using Microsoft.Extensions.Options;
using VendorGateway.Enums;

namespace VendorGateway.Configuration
{
    public class VendorsConfiguration
    {
        private readonly IOptions<VendorSettings> _options;
        private readonly List<VendorDetails> _vendors;

        public VendorsConfiguration() { }
        public VendorsConfiguration(IOptions<VendorSettings> options)
        {
            _options = options;
            _vendors = options?.Value?.VendorDetails ?? [];
        }

        public List<VendorDetails> GetAll() => _vendors;
        public VendorDetails GetDefaultVendor() => Get(Vendors.FakeStore);
        public VendorDetails Get(Vendors vendor) => _vendors?.FirstOrDefault(v => v.Name == vendor.ToString()) ?? throw new KeyNotFoundException($"Vendor '{vendor}' not found.");
    }
}
