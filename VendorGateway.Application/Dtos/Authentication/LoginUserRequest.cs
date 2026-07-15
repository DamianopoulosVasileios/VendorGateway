namespace VendorGateway.Application.Dtos.Authentication
{
    public sealed record LoginUserRequest
    {
        public string Username { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}
