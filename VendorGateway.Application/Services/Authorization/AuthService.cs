using System.Text.Json;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Dtos.Authentication;
using VendorGateway.Application.Interfaces;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Jobs.Commands;
using VendorGateway.Application.Jobs.Entities;
using static VendorGateway.Application.Jobs.DTOs.AsynchronousAPI;

namespace VendorGateway.Application.Services.Authorization
{
    public class AuthService(
        IAuthorizationQueries authorizationQueries,
        IAuthorizationCommands authorizationCommands,
        IAuthenticationTypeService authenticationTypeService,
        IPasswordHasherService passwordHasherService,
        IJobCommands jobCommands) : IAuthService
    {
        public async Task<string?> LoginAsync(LoginAccountRequest request)
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

        public async Task<bool> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
        {
            var userRequest = request with
            {
                Password = passwordHasherService.Hash(request.Password)
            };

            var userId = await authorizationCommands.RegisterUserAsync(userRequest);
            if (userId == null)
                return false;

            var payload = new CreateAccountJobPayload(userId.Value, new CreateAccountRequest(request.Email));
            var job = new Job { Type = JobType.CreateAccount, Payload = JsonSerializer.Serialize(payload) };
            await jobCommands.CreateAsync(job, ct);

            return true;
        }
    }
}
