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
        private int AccountId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetOrder([FromServices] IGetOrderService service, int orderId, CancellationToken ct)
        {
            var order = await service.GetAsync(AccountId, orderId, ct);
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
            var payload = new CreateOrderJobPayload(AccountId, idempotencyKey, mappedOrder);
            var job = new Job { Type = JobType.CreateOrder, Payload = JsonSerializer.Serialize(payload) };

            await jobCommands.CreateAsync(job, ct);
            return Accepted();
        }

        [HttpPut("{orderId:int}")]
        public async Task<IActionResult> UpdateOrder([FromServices] IUpdateOrderService service, int orderId, ApiUpdateOrderRequest request, CancellationToken ct)
        {
            var mappedOrder = request.ToDto();
            await service.UpdateAsync(AccountId, orderId, mappedOrder, ct);
            return Ok();
        }

        [HttpDelete("{orderId:int}")]
        public async Task<IActionResult> DeleteByIdOrder([FromServices] IDeleteOrderService service, int orderId, CancellationToken ct)
        {
            await service.DeleteAsync(AccountId, orderId, ct);
            return Ok();
        }

        [HttpPost("execute/{orderId:int}")]
        public async Task<IActionResult> ExecuteOrder([FromServices] IExecuteOrderService service, int orderId, CancellationToken ct)
        {
            await service.ExecuteAsync(AccountId, orderId, ct);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromServices] IOrderQueries orderQueries, [FromServices] IAccountExistenceGuard accountExistenceGuard, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(AccountId, ct);

            var orders = await orderQueries.GetAsync(AccountId, ct);
            var mappedResponse = orders.Select(x => x.ToApiResponse()).ToList();
            return Ok(mappedResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOrders([FromServices] IOrderCommands orderCommands, [FromServices] IAccountExistenceGuard accountExistenceGuard, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(AccountId, ct);

            await orderCommands.DeleteAsync(AccountId, ct);
            return Ok();
        }

        [HttpGet("orderItems")]
        public async Task<IActionResult> GetOrderItems([FromServices] IOrderQueries orderQueries, [FromServices] IAccountExistenceGuard accountExistenceGuard, CancellationToken ct)
        {
            await accountExistenceGuard.EnsureExistsAsync(AccountId, ct);

            var orders = await orderQueries.GetAsync(AccountId, ct);
            var orderIds = orders.Select(x => x.Id);

            var orderItems = await orderQueries.GetOrderItemsAsync(orderIds, ct);
            var mappedResponse = orderItems.Select(x => x.ToApiResponse()).ToList();
            return Ok(mappedResponse);
        }
    }
}
