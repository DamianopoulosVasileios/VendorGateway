using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Account
{
    public class CreateAccountService(IAccountsApiClient usersApiClient, IAccountCommands accountCommands) : ICreateAccountService
    {
        public async Task CreateAsync(CreateAccountRequest request, CancellationToken ct)
        {
            var results = await usersApiClient.CreateAsync(request, ct);
            if (results.id == 0)
            {
                throw new InvalidOperationException($"The account {request.id} could not be saved at vendor");
            }

            await accountCommands.CreateAsync(request.id, request.email, ct);
        }
    }
}
