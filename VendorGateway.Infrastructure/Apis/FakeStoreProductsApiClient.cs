using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Infrastructure.Apis.Contracts.Responses;
using VendorGateway.Infrastructure.APIs;
using VendorGateway.Infrastructure.APIs.Configuration;
using VendorGateway.Infrastructure.Enums;
using VendorGateway.Infrastructure.Helpers;
using VendorGateway.Infrastructure.Mappers;

namespace VendorGateway.Infrastructure.API
{
    public sealed class FakeStoreProductsApiClient : VendorApiClientBase, IProductsApiClient
    {
        protected override Vendors Vendor => Vendors.FakeStore;
        private readonly IApiResponseReader _apiResponseReader;

        public FakeStoreProductsApiClient(IApiResponseReader apiResponseReader, IHttpClientFactory factory, VendorsConfiguration configuration) 
            : base(factory, configuration)
        {
            _apiResponseReader = apiResponseReader;
        }

        public async Task<IEnumerable<GetProductsResponse>> GetAllAsync(CancellationToken ct)
        {
            var url = Config.Products.GetAll;

            var call = await Client.GetAsync(url, ct);
            var response = await _apiResponseReader.ReadAsync<IEnumerable<FakeStoreGetProductsResponse>>(call, ct);
            return ProductMapper.ToApi(response);
        }

        public async Task<HttpResponseMessage> DeleteByIdAsync(int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Products.Delete, id);

            var response = await Client.DeleteAsync(url, ct);
            var result = _apiResponseReader.EnsureSuccessStatusCode(response);

            return result;
        }
    }
}