using System.Text.Json;
using VendorGateway.Application.Common;
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
        public async Task<Result<string>> LoginAsync(LoginAccountRequest request)
        {
            var user = await authorizationQueries.GetUserByUsernameAsync(request.Username);
            if (user is null)
                return Result.Failure<string>(Error.Unauthorized("Invalid username or password."));

            var validPassword = passwordHasherService.Verify(
                request.Password,
                user.PasswordHash);

            if (!validPassword)
                return Result.Failure<string>(Error.Unauthorized("Invalid username or password."));

            return Result.Success(authenticationTypeService.GenerateToken(user.Id.ToString()));
        }

        public async Task<Result> RegisterAsync(RegisterUserRequest request, CancellationToken ct)
        {
            var userRequest = request with
            {
                Password = passwordHasherService.Hash(request.Password)
            };

            var userId = await authorizationCommands.RegisterUserAsync(userRequest);
            if (userId == null)
                return Result.Failure(Error.Conflict($"Username '{request.Username}' is already taken."));

            var payload = new CreateAccountJobPayload(userId.Value, new CreateAccountRequest(request.Email));
            var job = new Job { Type = JobType.CreateAccount, Payload = JsonSerializer.Serialize(payload) };
            await jobCommands.CreateAsync(job, ct);

            return Result.Success();
        }
    }
}
