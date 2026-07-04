using VendorGateway.APIs;
using VendorGateway.Configuration;
using VendorGateway.Contracts.Product.Responses;
using VendorGateway.Enums;
using VendorGateway.Infrastructure.Contracts.Product.Responses;
using VendorGateway.Interfaces;
using VendorGateway.Mappers;

namespace VendorGateway.API
{
    public sealed class FakeStoreProductsApiClient : VendorApiClientBase, IProductsApiClient
    {
        protected override Vendors Vendor => Vendors.FakeStore;
        private readonly IApiResponseReader _apiResponseReader;

        public FakeStoreProductsApiClient(IApiResponseReader apiResponseReader, IHttpClientFactory factory, VendorsConfiguration configuration) : base(factory, configuration)
        {
            _apiResponseReader = apiResponseReader;
        }

        public async Task<IEnumerable<ApiGetProductsResponse>> GetAllAsync(CancellationToken ct)
        {
            var url = Config.Products.GetAll;

            var call = await Client.GetAsync(url, ct);
            var response = await _apiResponseReader.ReadAsync<IEnumerable<FakeStoreGetProductsResponse>>(call, ct);
            return ApiAndFakeStoreProductMapper.ToApi(response);
        }
    }
}