using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;

namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IAuthorizationCommands
    {
        Task<int?> RegisterUserAsync(RegisterUserRequest request);
    }
}