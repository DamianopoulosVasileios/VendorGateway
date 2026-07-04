using VendorGateway.Contracts.Product.Responses;

namespace VendorGateway.Interfaces
{
    public interface IProductsApiClient
    {
        Task<IEnumerable<ApiGetProductsResponse>> GetAllAsync(CancellationToken ct);
    }
}