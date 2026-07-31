using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;
using VendorGateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Interfaces.CommandsQueries;

namespace VendorGateway.Infrastructure.Repositories.Authorization
{
    public class AuthorizationQueries(AppDbContext context) : IAuthorizationQueries
    {
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Username == username);
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
            return user;
        }
    }
}
