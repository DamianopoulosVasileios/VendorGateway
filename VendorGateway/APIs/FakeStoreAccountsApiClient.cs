using VendorGateway.Common;
using VendorGateway.Configuration;
using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;
using VendorGateway.Enums;
using VendorGateway.Interfaces;
using VendorGateway.Mappers;

namespace VendorGateway.APIs
{
    public sealed class FakeStoreAccountsApiClient : VendorApiClientBase, IAccountsApiClient
    {
        protected override Vendors Vendor => Vendors.FakeStore;
        private readonly IApiResponseReader _apiResponseReader;

        public FakeStoreAccountsApiClient(IApiResponseReader apiResponseReader, IHttpClientFactory factory, VendorsConfiguration configuration)
            : base(factory, configuration)
        {
            _apiResponseReader = apiResponseReader;
        }

        public async Task<ApiGetAccountResponse> GetByIdAsync(int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Users.Get, id);

            var call = await Client.GetAsync(url, ct);
            var response = await _apiResponseReader.ReadAsync<FakeStoreGetAccountResponse>(call, ct);

            return ApiAndFakeStoreAccountMappers.ToApi(response);
        }

        public async Task<ApiCreateAccountResponse> CreateAsync(ApiCreateAccountRequest apiRequest, CancellationToken ct)
        {
            var request = ApiAndFakeStoreAccountMappers.ToFakeStore(apiRequest);

            var url = Config.Users.Create;
            var call = await Client.PostAsJsonAsync(url, request, ct);
            var response = await _apiResponseReader.ReadAsync<FakeStoreCreateAccountResponse>(call, ct);

            return ApiAndFakeStoreAccountMappers.ToApi(response);
        }

        public async Task<ApiUpdateAccountResponse> UpdateAsync(ApiUpdateAccountRequest apiRequest, int id, CancellationToken ct)
        {
            var request = ApiAndFakeStoreAccountMappers.ToFakeStore(apiRequest);

            var url = UrlResolver.Resolve(Config.Users.Update, id);
            var call = await Client.PutAsJsonAsync(url, request, ct);
            var response = await _apiResponseReader.ReadAsync<FakeStoreUpdateAccountResponse>(call, ct);

            return ApiAndFakeStoreAccountMappers.ToApi(response);
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Users.Delete, id);

            var response = await Client.DeleteAsync(url, ct);
            var result = _apiResponseReader.EnsureSuccessStatusCode(response);

            return result;
        }
    }
}