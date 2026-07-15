namespace VendorGateway.Application.Dtos.Authentication
{
    public sealed record RegisterUserRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
