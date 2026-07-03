using VendorGateway.Common;
using VendorGateway.Configuration;
using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;
using VendorGateway.Enums;
using VendorGateway.Interfaces;

namespace VendorGateway.APIs
{
    public sealed class FakeStoreAccountsApiClient : VendorApiClientBase, IUsersApiClient
    {
        protected override Vendors Vendor => Vendors.FakeStore;

        public FakeStoreAccountsApiClient(IHttpClientFactory factory, VendorsConfiguration configuration)
            : base(factory, configuration)
        {
        }

        public async Task<GetAccountResponse> GetAsync(int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Users.Get, id);

            var response = await Client.GetAsync(url, ct);
            return await Apis.Response<GetAccountResponse>(response, ct);
        }

        public async Task<CreateAccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct)
        {
            var url = Config.Users.Create;

            var response = await Client.PostAsJsonAsync(url, request, ct);
            return await Apis.Response<CreateAccountResponse>(response, ct);
        }

        public async Task<UpdateAccountResponse> UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Users.Update, id);

            var response = await Client.PutAsJsonAsync(url, request, ct);
            return await Apis.Response<UpdateAccountResponse>(response, ct);
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