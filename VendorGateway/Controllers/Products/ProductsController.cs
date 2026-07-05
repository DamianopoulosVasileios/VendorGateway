using Microsoft.AspNetCore.Mvc;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Interfaces;
using VendorGateway.Mappers;

namespace VendorGateway.Controllers.Products
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductQueries productQueries, IProductCommands productCommands, IProductsApiClient productsApiClient) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProducts(CancellationToken ct)
        {
            var results = await productQueries.GetAsync(ct);
            return Ok(results);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteProducts(CancellationToken ct)
        {
            await productCommands.DeleteAsync(ct);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProducts(CancellationToken ct)
        {
            var productsFromStore = await productsApiClient.GetAllAsync(ct);
            var productsToPersist = ProductMappers.Map(productsFromStore);

            try
            {
                var result = await productCommands.AddRangeAsync(productsToPersist, ct);
                if (!result)
                    return BadRequest("Failed to persist products.");
            }
            catch { }

            return Ok();
        }
    }
}
