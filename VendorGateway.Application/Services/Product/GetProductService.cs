using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Services.Product
{
    public class GetProductService(IProductQueries productQueries) : IGetProductService
    {
        public async Task<List<Entities.Product>> GetAsync(CancellationToken ct)
        {
            var results = await productQueries.GetAsync(ct);
            return results;
        }
    }
}
