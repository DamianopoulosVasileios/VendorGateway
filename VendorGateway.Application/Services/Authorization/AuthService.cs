using System;
using System.Collections.Generic;
using System.Text;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;
using VendorGateway.Application.Interfaces;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Authorization
{
    public class AuthService(
        IAuthorizationQueries authorizationQueries,
        IAuthorizationCommands authorizationCommands,
        IAuthenticationTypeService authenticationTypeService,
        IPasswordHasherService passwordHasherService) : IAuthService
    {
        public async Task<string?> LoginAsync(LoginUserRequest request)
        {
            var user = await authorizationQueries.GetUserByUsernameAsync(request.Username);
            if (user is null)
                return null;

            var validPassword = passwordHasherService.Verify(
                request.Password,
                user.PasswordHash);

            if (!validPassword)
                return null;

            return authenticationTypeService.GenerateToken(user.Id.ToString());
        }

        public async Task<bool> RegisterAsync(RegisterUserRequest request)
        {
            var userRequest = request with 
            {
                Username = request.Username,
                Password = passwordHasherService.Hash(request.Password)
            };

            var success = await authorizationCommands.RegisterUserAsync(userRequest);
            return success;
        }
    }
}
