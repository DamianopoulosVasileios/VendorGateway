namespace VendorGateway.Common
{
    public static class FakeStoreApis
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


    public static class FakeStoreDataGenerator
    {
        public static string GenerateUsername(object value)
                    => $"user_{(value?.ToString() ?? "default").Split('@')[0]}_{Guid.NewGuid().ToString("N")[..6]}";

        public static string GeneratePassword()
            => "Temp#12345!" + Guid.NewGuid().ToString("N")[..4];

        public static string GenerateEmail(object value)
            => $"{value}@fakestore.com";
    }
}
