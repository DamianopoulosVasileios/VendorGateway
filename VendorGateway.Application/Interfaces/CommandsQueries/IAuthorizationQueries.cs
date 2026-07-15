using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;

namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IAuthorizationQueries
    {
        Task<User?> GetUserByUsernameAsync(string username);
    }
}