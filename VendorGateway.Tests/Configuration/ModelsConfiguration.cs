using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using VendorGateway.Configuration;
using VendorGateway.Enums;
using YamlDotNet.Serialization;

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

            var options = Options.Create(settings);
            return new VendorsConfiguration(options);
        }

        public static VendorsConfiguration EmptyVendorsConfiguration()
        {
            var settings = new VendorSettings
            {
                VendorDetails = []
            };

            var options = Options.Create(settings);
            return new VendorsConfiguration(options);
        }
    }
}
