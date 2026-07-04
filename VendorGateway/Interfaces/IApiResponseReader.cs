namespace VendorGateway.Interfaces
{
    public interface IApiResponseReader
    {
        Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct);
        HttpResponseMessage EnsureSuccessStatusCode(HttpResponseMessage response);
    }
}
