using System.Net.Http.Json;
using VendorGateway.Application.Interfaces.ApiClient;

namespace VendorGateway.Infrastructure.Helpers
{
    public sealed class ApiResponseReader : IApiResponseReader
    {
        public async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct)
                   ?? throw new InvalidOperationException("Response was empty.");
        }
        public HttpResponseMessage EnsureSuccessStatusCode(HttpResponseMessage response)
        {
            return response.EnsureSuccessStatusCode();
        }
    }

    public static class UrlResolver
    {
        public static string Resolve(string template, object value)
        {
            return template.Replace("{id}", value.ToString());
        }
    }
}
