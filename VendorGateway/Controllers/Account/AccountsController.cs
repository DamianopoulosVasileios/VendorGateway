using Microsoft.AspNetCore.Mvc;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;

namespace VendorGateway.Controllers.Account
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAccount([FromServices] IGetAccountService getAccountService, int id, CancellationToken ct)
        {
            var result = await getAccountService.GetAsync(id, ct);
            var mappedResponse = new ApiGetAccountResponse(result!.Id, result.Email, result.Orders, result.CreatedAt, result.UpdatedAt);
            return Ok(mappedResponse);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromServices] ICreateAccountService createAccountService, ApiCreateAccountRequest request, CancellationToken ct)
        {
            var mapped = new CreateAccountRequest(request.id, request.email);
            await createAccountService.CreateAsync(mapped, ct);
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAccount([FromServices] IUpdateAccountService updateAccountService, int id, ApiUpdateAccountRequest request, CancellationToken ct)
        {
            var mapped = new UpdateAccountRequest(request.id);
            await updateAccountService.UpdateAsync(mapped, id, ct);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAccount([FromServices] IDeleteAccountService deleteAccountService, int id, CancellationToken ct)
        {
            await deleteAccountService.DeleteAsync(id, ct);
            return Ok();
        }
    }
}
