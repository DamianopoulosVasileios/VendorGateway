using Microsoft.AspNetCore.Mvc;
using VendorGateway.Contracts.Account.Requests;

namespace VendorGateway.Controllers.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAccount(Guid id, UpdateAccountRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAccount(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
