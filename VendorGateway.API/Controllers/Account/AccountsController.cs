using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VendorGateway.API.Contracts.Account.Requests;
using VendorGateway.API.Contracts.Account.Responses;
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
            var mappedResponse = new ApiGetAccountResponse(result!.Id, result.Email, result.Orders, result.CreatedAt, result.UpdatedAt);
            return Ok(mappedResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccount([FromServices] IUpdateAccountService updateAccountService, ApiUpdateAccountRequest request, CancellationToken ct)
        {
            var mapped = new UpdateAccountRequest(request.email);
            await updateAccountService.UpdateAsync(mapped, AccountId, ct);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount([FromServices] IDeleteAccountService deleteAccountService, CancellationToken ct)
        {
            await deleteAccountService.DeleteAsync(AccountId, ct);
            return Ok();
        }
    }
}
