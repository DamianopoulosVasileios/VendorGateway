using VendorGateway.Common;
using VendorGateway.Configuration;
using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;
using VendorGateway.Enums;
using VendorGateway.Infrastructure.Contracts.Account.Responses;
using VendorGateway.Interfaces;
using VendorGateway.Mappers;

namespace VendorGateway.APIs
{
    public sealed class FakeStoreAccountsApiClient : VendorApiClientBase, IUsersApiClient
    {
        protected override Vendors Vendor => Vendors.FakeStore;

        public FakeStoreAccountsApiClient(IHttpClientFactory factory, VendorsConfiguration configuration)
            : base(factory, configuration)
        {
        }

        public async Task<ApiGetAccountResponse> GetAsync(int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Users.Get, id);

            var call = await Client.GetAsync(url, ct);
            var response = await FakeStoreApis.Response<FakeStoreGetAccountResponse>(call, ct);

            return ApiAndFakeStoreAccountMappers.ToApi(response);
        }

        public async Task<ApiCreateAccountResponse> CreateAsync(ApiCreateAccountRequest apiRequest, CancellationToken ct)
        {
            var request = ApiAndFakeStoreAccountMappers.ToFakeStore(apiRequest);

            var url = Config.Users.Create;
            var call = await Client.PostAsJsonAsync(url, request, ct);
            var response = await FakeStoreApis.Response<FakeStoreCreateAccountResponse>(call, ct);

            return ApiAndFakeStoreAccountMappers.ToApi(response);
        }

        public async Task<ApiUpdateAccountResponse> UpdateAsync(ApiUpdateAccountRequest apiRequest, int id, CancellationToken ct)
        {
            var request = ApiAndFakeStoreAccountMappers.ToFakeStore(apiRequest);

            var url = UrlResolver.Resolve(Config.Users.Update, id);
            var call = await Client.PutAsJsonAsync(url, request, ct);
            var response = await FakeStoreApis.Response<FakeStoreUpdateAccountResponse>(call, ct);

            return ApiAndFakeStoreAccountMappers.ToApi(response);
        }

        public async Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Users.Delete, id);

            var response = await Client.DeleteAsync(url, ct);
            var result = response.EnsureSuccessStatusCode();

            return result;
        }
    }
}