using VendorGateway.Contracts.Product.Responses;

namespace VendorGateway.Interfaces
{
    public interface IProductsApiClient
    {
        Task<IEnumerable<GetProductsResponse>> GetAllAsync(CancellationToken ct);
    }
}