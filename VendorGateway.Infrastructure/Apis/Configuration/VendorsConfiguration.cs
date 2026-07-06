using Microsoft.Extensions.Options;
using VendorGateway.Infrastructure.Enums;

namespace VendorGateway.Infrastructure.APIs.Configuration
{
    public class VendorsConfiguration
    {
        private readonly IOptionsMonitor<VendorSettings> _options;

        public VendorsConfiguration() { }
        public VendorsConfiguration(IOptionsMonitor<VendorSettings> options)
        {
            _options = options;
        }

        public List<VendorDetails> GetAll()
            => _options.CurrentValue.VendorDetails;

        public VendorDetails Get(Vendors vendor)
            => _options.CurrentValue.VendorDetails
                ?.FirstOrDefault(v => v.Name == vendor.ToString())
                ?? throw new KeyNotFoundException($"Vendor '{vendor}' not found.");

    }
}
