namespace VendorGateway.Application.Dtos.Authentication
{
    public sealed class LoginAccountResponse
    {
        public string Token { get; init; } = string.Empty;

        public DateTime ExpiresAt { get; init; }
    }
}
