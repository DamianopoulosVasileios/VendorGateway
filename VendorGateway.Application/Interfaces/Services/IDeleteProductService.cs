using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IDeleteProductService
    {
        Task<Result> DeleteAsync(CancellationToken ct);
    }
}
