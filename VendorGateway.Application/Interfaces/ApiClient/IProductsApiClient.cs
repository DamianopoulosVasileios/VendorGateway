using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.ApiClient
{
    public interface IProductsApiClient
    {
        Task<IEnumerable<GetProductsResponse>> GetAllAsync(CancellationToken ct);
        Task<HttpResponseMessage> DeleteByIdAsync(int id, CancellationToken ct);
    }
}