using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VendorGateway.API.Contracts.Account.Requests;
using VendorGateway.API.Contracts.Account.Responses;
using VendorGateway.API.Extensions;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.API.Controllers.Account
{
    [Authorize(Policy = "ExistingUser")]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private int AccountId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAccount([FromServices] IGetAccountService getAccountService, CancellationToken ct)
        {
            var result = await getAccountService.GetAsync(AccountId, ct);
            return result.ToActionResult(a => new ApiGetAccountResponse(a.Id, a.Email, a.Orders, a.CreatedAt, a.UpdatedAt));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccount([FromServices] IUpdateAccountService updateAccountService, ApiUpdateAccountRequest request, CancellationToken ct)
        {
            var mapped = new UpdateAccountRequest(request.email);
            var result = await updateAccountService.UpdateAsync(mapped, AccountId, ct);
            return result.ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount([FromServices] IDeleteAccountService deleteAccountService, CancellationToken ct)
        {
            var result = await deleteAccountService.DeleteAsync(AccountId, ct);
            return result.ToActionResult();
        }
    }
}
