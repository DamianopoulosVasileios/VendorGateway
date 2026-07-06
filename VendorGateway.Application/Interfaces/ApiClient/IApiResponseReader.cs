namespace VendorGateway.Application.Interfaces.ApiClient
{
    public interface IApiResponseReader
    {
        Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct);
        HttpResponseMessage EnsureSuccessStatusCode(HttpResponseMessage response);
    }
}
