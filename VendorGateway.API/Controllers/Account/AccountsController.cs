using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using VendorGateway.API.Contracts.Account.Requests;
using VendorGateway.API.Contracts.Account.Responses;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Jobs.Commands;
using VendorGateway.Application.Jobs.Entities;
using static VendorGateway.Application.Jobs.DTOs.AsynchronousAPI;

namespace VendorGateway.API.Controllers.Account
{
    [Authorize(Policy = "ExistingUser")]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(IJobCommands jobCommands) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAccount([FromServices] IGetAccountService getAccountService, CancellationToken ct)
        {
            var result = await getAccountService.GetAsync(userId, ct);
            var mappedResponse = new ApiGetAccountResponse(result!.Id, result.Email, result.Orders, result.CreatedAt, result.UpdatedAt);
            return Ok(mappedResponse);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateAccount(ApiCreateAccountRequest request, CancellationToken ct)
        {
            var mappedRequest = new CreateAccountRequest(request.id, request.email);
            var payload = new CreateAccountJobPayload(mappedRequest);
            var job = new Job { Type = JobType.CreateAccount, Payload = JsonSerializer.Serialize(payload) };

            await jobCommands.CreateAsync(job, ct);
            return Accepted();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccount([FromServices] IUpdateAccountService updateAccountService, ApiUpdateAccountRequest request, CancellationToken ct)
        {
            var mapped = new UpdateAccountRequest();
            await updateAccountService.UpdateAsync(mapped, userId, ct);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount([FromServices] IDeleteAccountService deleteAccountService, CancellationToken ct)
        {
            await deleteAccountService.DeleteAsync(userId, ct);
            return Ok();
        }
    }
}
