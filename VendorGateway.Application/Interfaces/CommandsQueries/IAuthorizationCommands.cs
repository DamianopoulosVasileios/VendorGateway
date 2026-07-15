using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;

namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IAuthorizationCommands
    {
        Task<bool> RegisterUserAsync(RegisterUserRequest request);
    }
}