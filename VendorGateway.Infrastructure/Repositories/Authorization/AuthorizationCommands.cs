using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace VendorGateway.Infrastructure.Repositories.Authorization
{
    public class AuthorizationCommands(AppDbContext context, IDbExceptionClassifier dbExceptionClassifier) : IAuthorizationCommands
    {
        public async Task<int?> RegisterUserAsync(RegisterUserRequest request)
        {
            var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);

            if (existingUser != null)
                return null;

            var user = new User
            {
                Username = request.Username,
                PasswordHash = request.Password
            };

            context.Users.Add(user);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (dbExceptionClassifier.IsUniqueConstraintViolation(ex))
            {
                return null;
            }

            return user.Id;
        }
    }
}
