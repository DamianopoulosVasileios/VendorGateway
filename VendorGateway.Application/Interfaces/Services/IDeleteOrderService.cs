using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IDeleteOrderService
    {
        Task<Result> DeleteAsync(int accountId, int id, CancellationToken ct);
    }
}
