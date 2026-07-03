using VendorGateway.Configuration;
using VendorGateway.Enums;

namespace VendorGateway.APIs
{
    public abstract class VendorApiClientBase
    {
        protected readonly IHttpClientFactory Factory;
        protected readonly VendorsConfiguration Configuration;

        protected abstract Vendors Vendor { get; }

        protected VendorApiClientBase(IHttpClientFactory factory, VendorsConfiguration configuration)
        {
            Factory = factory;
            Configuration = configuration;
        }

        protected HttpClient Client => Factory.CreateClient(Vendor.ToString());
        protected VendorDetails Config => Configuration.Get(Vendor);
    }
}
