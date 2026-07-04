using Microsoft.AspNetCore.Mvc;
using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Interfaces;

namespace VendorGateway.Controllers.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IAccountsApiClient users) : ControllerBase
    {

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAccount(int id)
        {
            var results = await users.GetAsync(id, CancellationToken.None);
            return Ok(results);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(ApiCreateAccountRequest request)
        {
            var results = await users.CreateAsync(request, CancellationToken.None);
            return Ok(results);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAccount(int id, ApiUpdateAccountRequest request)
        {
            var results = await users.UpdateAsync(request, id, CancellationToken.None);
            return Ok(results);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var results = await users.DeleteAsync(id, CancellationToken.None);
            return StatusCode((int)results.StatusCode);
        }
    }
}
