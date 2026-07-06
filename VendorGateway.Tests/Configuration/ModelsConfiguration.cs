

using VendorGateway.Infrastructure.APIs.Configuration;
using VendorGateway.Infrastructure.Enums;

namespace VendorGateway.Tests.Configuration
{
    public static class ModelsConfiguration
    {
        public static VendorsConfiguration VendorsConfiguration()
        {
            var settings = new VendorSettings
            {
                VendorDetails =
                [
                    new VendorDetails
                {
                    Name = Vendors.FakeStore.ToString(),
                    ApiUrl = "https://fake.com",
                    TimeoutSeconds = 30
                },
                new VendorDetails
                {
                    Name = Vendors.FutureStore.ToString(),
                    ApiUrl = "https://amazon.com",
                    TimeoutSeconds = 30
                }
                ]
            };

            var options = new OptionsMonitorAdapter<VendorSettings>(settings);
            return new VendorsConfiguration(options);
        }

        public static VendorsConfiguration EmptyVendorsConfiguration()
        {
            var settings = new VendorSettings
            {
                VendorDetails = []
            };

            var options = new OptionsMonitorAdapter<VendorSettings>(settings);
            return new VendorsConfiguration(options);
        }
    }
}
