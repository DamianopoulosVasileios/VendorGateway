using Microsoft.AspNetCore.Mvc;
using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Interfaces;

namespace VendorGateway.Controllers.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IAccountsApiClient usersApiClient, IAccountCommands accountCommands, IAccountQueries accountQueries) : ControllerBase
    {

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAccount(int id, CancellationToken ct)
        {
            var user = await CheckAccountExists(id, ct);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(ApiCreateAccountRequest request, CancellationToken ct)
        {
            var results = await usersApiClient.CreateAsync(request, ct);
            if (results.id == 0)
            {
                throw new InvalidOperationException($"The account {request.id} could not be saved at vendor");
            }

            await accountCommands.CreateAsync(request.id, request.email, ct);
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAccount(int id, ApiUpdateAccountRequest request, CancellationToken ct)
        {
            await CheckAccountExists(id, ct);

            var results = await usersApiClient.UpdateAsync(request, id, ct);
            return Ok(results);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAccount(int id, CancellationToken ct)
        {
            await CheckAccountExists(id, ct);

            var results = await usersApiClient.DeleteAsync(id, ct);

            await accountCommands.DeleteAsync(id, ct);

            return StatusCode((int)results.StatusCode);
        }

        private async Task<IReadOnlyList<Application.Entities.Account>> CheckAccountExists(int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");

            return account;
        }
    }
}
