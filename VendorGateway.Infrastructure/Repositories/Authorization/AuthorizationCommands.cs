using System;
using System.Collections.Generic;
using System.Text;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace VendorGateway.Infrastructure.Repositories.Authorization
{
    public class AuthorizationCommands(AppDbContext context) : IAuthorizationCommands
    {
        public async Task<bool> RegisterUserAsync(RegisterUserRequest request)
        {
            var existingUser = await context.Users.FirstOrDefaultAsync(x => x.Username == request.Username);

            if (existingUser != null)
                return false;

            var user = new User
            {
                Username = request.Username,
                PasswordHash = request.Password
            };

            context.Users.Add(user);

            await context.SaveChangesAsync();

            return true;
        }
    }
}
