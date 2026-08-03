using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos.Authentication;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Result<string>> LoginAsync(LoginAccountRequest request);
        Task<Result> RegisterAsync(RegisterUserRequest request, CancellationToken ct);
    }

}
