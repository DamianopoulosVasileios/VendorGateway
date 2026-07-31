using VendorGateway.Application.Dtos.Authentication;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(LoginAccountRequest request);
        Task<bool> RegisterAsync(RegisterUserRequest request, CancellationToken ct);
    }

}
