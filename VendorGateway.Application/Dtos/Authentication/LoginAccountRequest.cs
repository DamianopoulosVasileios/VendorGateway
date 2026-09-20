using System.ComponentModel.DataAnnotations;

namespace VendorGateway.Application.Dtos.Authentication
{
    public sealed record LoginAccountRequest
    {
        [Required]
        public string Username { get; init; } = string.Empty;

        [Required]
        public string Password { get; init; } = string.Empty;
    }
}
