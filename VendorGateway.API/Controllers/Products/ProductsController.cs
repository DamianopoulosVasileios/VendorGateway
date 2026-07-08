using Microsoft.AspNetCore.Mvc;
using VendorGateway.API.Mappers;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.API.Controllers.Products
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController() : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromServices] IGetProductService service, CancellationToken ct)
        {
            var products = await service.GetAsync(ct);
            var mappedResponse = products.Select(p => p.ToApiResponse()).ToList();
            return Ok(mappedResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProducts([FromServices] IDeleteProductService service, CancellationToken ct)
        {
            await service.DeleteAsync(ct);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProducts([FromServices] ICreateProductService service, CancellationToken ct)
        {
            var result = await service.UpdateAsync(ct);
            return Ok(result);
        }
    }
}
