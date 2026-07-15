using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using VendorGateway.API.Contracts.Order.Requests;
using VendorGateway.API.Filters;
using VendorGateway.API.Mappers;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Jobs.Commands;
using VendorGateway.Application.Jobs.Entities;
using static VendorGateway.Application.Jobs.DTOs.AsynchronousAPI;

namespace VendorGateway.API.Controllers.Order
{
    [Authorize(Policy = "ExistingUser")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController(IJobCommands jobCommands) : ControllerBase
    {

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrder([FromServices] IGetOrderService service, int orderId, CancellationToken ct)
        {
            var order = await service.GetAsync(userId, orderId, ct);
            var mappedResult = order.ToApiResponse();
            return Ok(mappedResult);
        }

        [HttpPost]
        [RequireIdempotencyKey]
        public async Task<IActionResult> CreateOrder(
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            ApiCreateOrderRequest request,
            CancellationToken ct)
        {
            var mappedOrder = request.ToDto();
            var payload = new CreateOrderJobPayload(idempotencyKey, mappedOrder);
            var job = new Job { Type = JobType.CreateOrder, Payload = JsonSerializer.Serialize(payload) };

            await jobCommands.CreateAsync(job, ct);
            return Accepted();
        }

        [HttpPut("{orderId:int}")]
        public async Task<IActionResult> UpdateOrder([FromServices] IUpdateOrderService service, int orderId, ApiUpdateOrderRequest request, CancellationToken ct)
        {
            var mappedOrder = request.ToDto();
            await service.UpdateAsync(accountId, orderId, mappedOrder, ct);
            return Ok();
        }

        [HttpDelete("{orderId:int}")]
        public async Task<IActionResult> DeleteByIdOrder([FromServices] IDeleteOrderService service, int orderId, CancellationToken ct)
        {
            await service.DeleteAsync(accountId, orderId, ct);
            return Ok();
        }

        [HttpPost("execute/{orderId:int}")]
        public async Task<IActionResult> ExecuteOrder([FromServices] IExecuteOrderService service, int orderId, CancellationToken ct)
        {
            await service.ExecuteAsync(accountId, orderId, ct);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromServices] IOrderQueries orderQueries, [FromServices] IAccountQueries accountQueries, CancellationToken ct)
        {
            await CheckAccountExists(accountQueries, ct);

            var orders = await orderQueries.GetAsync(accountId, ct);
            var mappedResponse = orders.Select(x => x.ToApiResponse()).ToList();
            return Ok(mappedResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOrders([FromServices] IOrderCommands orderCommands, [FromServices] IAccountQueries accountQueries, CancellationToken ct)
        {
            await CheckAccountExists(accountQueries, ct);

            await orderCommands.DeleteAsync(accountId, ct);
            return Ok();
        }

        [HttpGet("orderItems")]
        public async Task<IActionResult> GetOrderItems([FromServices] IOrderQueries orderQueries, [FromServices] IAccountQueries accountQueries, CancellationToken ct)
        {
            await CheckAccountExists(accountQueries, ct);

            var orders = await orderQueries.GetAsync(accountId, ct);
            var orderIds = orders.Select(x => x.Id);

            var orderItems = await orderQueries.GetOrderItemsAsync(orderIds, ct);
            var mappedResponse = orderItems.Select(x => x.ToApiResponse()).ToList();
            return Ok(mappedResponse);
        }

        private static async Task CheckAccountExists(IAccountQueries accountQueries, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");
        }
    }
}
