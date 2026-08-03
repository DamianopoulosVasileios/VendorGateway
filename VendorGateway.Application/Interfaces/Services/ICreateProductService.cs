using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateProductService
    {
        Task<Result<bool>> UpdateAsync(CancellationToken ct);
    }
}
