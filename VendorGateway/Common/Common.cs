namespace VendorGateway.Common
{
    public static class Apis
    {
        public static async Task<T> Response<T>(HttpResponseMessage response, CancellationToken ct) where T : class
        {
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct) ?? throw new InvalidOperationException("Response was empty.");
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
