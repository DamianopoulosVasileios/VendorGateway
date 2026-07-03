using VendorGateway.APIs;
using VendorGateway.Common;
using VendorGateway.Configuration;
using VendorGateway.Contracts.Product.Responses;
using VendorGateway.Enums;
using VendorGateway.Interfaces;

namespace VendorGateway.API
{
    public sealed class FakeStoreProductsApiClient : VendorApiClientBase, IProductsApiClient
    {
        protected override Vendors Vendor => Vendors.FakeStore;

        public FakeStoreProductsApiClient(IHttpClientFactory factory, VendorsConfiguration configuration) : base(factory, configuration)
        {
        }

        public async Task<IEnumerable<GetProductsResponse>> GetAllAsync(CancellationToken ct)
        {
            var url = Config.Products.GetAll;

            var response = await Client.GetAsync(url, ct);
            return await Apis.Response<IEnumerable<GetProductsResponse>>(response, ct);
        }
    }
}