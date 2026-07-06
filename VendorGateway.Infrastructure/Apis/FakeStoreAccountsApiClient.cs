using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Enums;
using VendorGateway.Application.Mappers;
using VendorGateway.Infrastructure.Enums;
using VendorGateway.Infrastructure.APIs.Configuration;
using VendorGateway.Infrastructure.Helpers;
using VendorGateway.Infrastructure.Apis.Contracts.Responses;
using System.Net.Http.Json;
using VendorGateway.Infrastructure.Mappers;

namespace VendorGateway.Infrastructure.APIs
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

        public async Task<GetAccountVendorResponse> GetByIdAsync(int id, CancellationToken ct)
        {
            var url = UrlResolver.Resolve(Config.Users.Get, id);

            var call = await Client.GetAsync(url, ct);
            var response = await _apiResponseReader.ReadAsync<FakeStoreGetAccountResponse>(call, ct);

            return AccountMappers.ToApi(response);
        }

        public async Task<CreateAccountVendorResponse> CreateAsync(CreateAccountRequest apiRequest, CancellationToken ct)
        {
            var request = AccountMappers.ToFakeStore(apiRequest);

            var url = Config.Users.Create;
            var call = await Client.PostAsJsonAsync(url, request, ct);
            var response = await _apiResponseReader.ReadAsync<FakeStoreCreateAccountResponse>(call, ct);

            return AccountMappers.ToApi(response);
        }

        public async Task<UpdateAccountVendorResponse> UpdateAsync(UpdateAccountRequest apiRequest, int id, CancellationToken ct)
        {
            var request = AccountMappers.ToFakeStore(apiRequest);

            var url = UrlResolver.Resolve(Config.Users.Update, id);
            var call = await Client.PutAsJsonAsync(url, request, ct);
            var response = await _apiResponseReader.ReadAsync<FakeStoreUpdateAccountResponse>(call, ct);

            return AccountMappers.ToApi(response);
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