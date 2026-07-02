using Microsoft.AspNetCore.Mvc;
using VendorGateway.Contracts.Order.Requests;

namespace VendorGateway.Controllers.Order
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateOrder(Guid id, UpdateOrderRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost("{id:guid}/execute")]
        public async Task<IActionResult> ExecuteOrder(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
