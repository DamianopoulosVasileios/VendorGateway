using System.ComponentModel.DataAnnotations;

namespace VendorGateway.Application.Dtos.Authentication
{
    public sealed record RegisterUserRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
