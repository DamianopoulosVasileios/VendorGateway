using Microsoft.AspNetCore.Mvc;
using VendorGateway.Contracts.Order.Requests;

namespace VendorGateway.Controllers.Order
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(ApiCreateOrderRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> UpdateOrder(int id, ApiUpdateOrderRequest request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPost("execute/{id:int}")]
        public async Task<IActionResult> ExecuteOrder(int id)
        {
            throw new NotImplementedException();
        }
    }
}
