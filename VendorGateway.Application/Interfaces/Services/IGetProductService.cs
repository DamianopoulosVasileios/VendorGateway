using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IGetProductService
    {
        Task<Result<List<Entities.Product>>> GetAsync(CancellationToken ct);
    }
}
