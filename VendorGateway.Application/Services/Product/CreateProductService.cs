using VendorGateway.Application.Interfaces.ApiClient;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Mappers;

namespace VendorGateway.Application.Services.Product
{
    public class CreateProductService(IProductsApiClient productsApiClient, IProductCommands productCommands) : ICreateProductService
    {
        public async Task<bool> CreateAsync(CancellationToken ct)
        {

            var productsFromStore = await productsApiClient.GetAllAsync(ct);
            var productsToPersist = ProductMappers.Map(productsFromStore);
            try
            {
                var result = await productCommands.AddRangeAsync(productsToPersist, ct);
                if (!result)
                    return false;
            }
            catch { }

            return true;
        }
    }
}
