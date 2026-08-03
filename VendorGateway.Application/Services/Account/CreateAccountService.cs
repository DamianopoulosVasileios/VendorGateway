using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class CreateAccountService(IAccountsApiClient usersApiClient, IAccountCommands accountCommands) : ICreateAccountService
    {
        public async Task<Result> CreateAsync(CreateAccountRequest request, int id, CancellationToken ct)
        {
            var results = await usersApiClient.CreateAsync(request, id, ct);
            if (results.id == 0)
            {
                return Result.Failure(Error.Conflict($"The account {id} could not be saved at vendor"));
            }

            return await accountCommands.CreateAsync(id, request.email, ct);
        }
    }
}
