using Microsoft.AspNetCore.Mvc;
using VendorGateway.API.Contracts.Order.Requests;
using VendorGateway.API.Filters;
using VendorGateway.API.Mappers;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.API.Controllers.Order
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder([FromServices] IGetOrderService service, int accountId, int id, CancellationToken ct)
        {
            var order = await service.GetAsync(accountId, id, ct);
            var mappedResult = order.ToApiResponse();
            return Ok(mappedResult);
        }

        [HttpPost]
        [RequireIdempotencyKey]
        public async Task<IActionResult> CreateOrder(
            [FromServices] ICreateOrderService service,
            [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
            ApiCreateOrderRequest request,
            CancellationToken ct)
        {
            var mappedOrder = request.ToDto();
            await service.CreateAsync(idempotencyKey, mappedOrder, ct);
            return Ok();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrder([FromServices] IUpdateOrderService service, int accountId, int id, ApiUpdateOrderRequest request, CancellationToken ct)
        {
            var mappedOrder = request.ToDto();
            await service.UpdateAsync(accountId, id, mappedOrder, ct);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteByIdOrder([FromServices] IDeleteOrderService service, int accountId, int id, CancellationToken ct)
        {
            await service.DeleteAsync(accountId, id, ct);
            return Ok();
        }

        [HttpPost("execute/{id:int}")]
        public async Task<IActionResult> ExecuteOrder([FromServices] IExecuteOrderService service, int accountId, int id, CancellationToken ct)
        {
            await service.ExecuteAsync(accountId, id, ct);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders([FromServices] IOrderQueries orderQueries, [FromServices] IAccountQueries accountQueries, int accountId, CancellationToken ct)
        {
            await CheckAccountExists(accountQueries, accountId, ct);

            var orders = await orderQueries.GetAsync(accountId, ct);
            var mappedResponse = orders.Select(x => x.ToApiResponse()).ToList();
            return Ok(mappedResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOrders([FromServices] IOrderCommands orderCommands, [FromServices] IAccountQueries accountQueries, int accountId, CancellationToken ct)
        {
            await CheckAccountExists(accountQueries, accountId, ct);

            await orderCommands.DeleteAsync(accountId, ct);
            return Ok();
        }
        [HttpGet("orderItems")]
        public async Task<IActionResult> GetOrderItems([FromServices] IOrderQueries orderQueries, [FromServices] IAccountQueries accountQueries, int accountId, CancellationToken ct)
        {
            await CheckAccountExists(accountQueries, accountId, ct);

            var orders = await orderQueries.GetAsync(accountId, ct);
            var orderIds = orders.Select(x => x.Id);

            var orderItems = await orderQueries.GetOrderItemsAsync(orderIds, ct);
            var mappedResponse = orderItems.Select(x => x.ToApiResponse()).ToList();
            return Ok(mappedResponse);
        }

        private static async Task CheckAccountExists(IAccountQueries accountQueries, int accountId, CancellationToken ct)
        {
            var account = await accountQueries.GetByIdsAsync([accountId], ct);
            if (account == null || account.Count == 0)
                throw new KeyNotFoundException($"Account with id {accountId} not found.");
        }
    }
}
