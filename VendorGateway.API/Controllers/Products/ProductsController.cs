using Microsoft.AspNetCore.Mvc;
using VendorGateway.API.Extensions;
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
            var result = await service.GetAsync(ct);
            return result.ToActionResult(products => products.Select(p => p.ToApiResponse()).ToList());
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProducts([FromServices] IDeleteProductService service, CancellationToken ct)
        {
            var result = await service.DeleteAsync(ct);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProducts([FromServices] ICreateProductService service, CancellationToken ct)
        {
            var result = await service.UpdateAsync(ct);
            return result.ToActionResult();
        }
    }
}
