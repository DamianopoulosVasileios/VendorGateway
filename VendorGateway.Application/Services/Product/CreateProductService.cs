using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Mappers;

namespace VendorGateway.Application.Services.Product
{
    public class CreateProductService(IProductsApiClient productsApiClient, IProductCommands productCommands) : ICreateProductService
    {
        public async Task<Result<bool>> UpdateAsync(CancellationToken ct)
        {
            var productsFromStore = await productsApiClient.GetAllAsync(ct);
            var productsToPersist = ProductMappers.Map(productsFromStore);
            var result = await productCommands.UpdateRangeAsync(productsToPersist, ct);
            return Result.Success(result);
        }
    }
}
