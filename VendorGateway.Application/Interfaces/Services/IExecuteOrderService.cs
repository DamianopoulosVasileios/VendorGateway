using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IExecuteOrderService
    {
        Task<Result> ExecuteAsync(int accountId, int id, CancellationToken ct);
    }
}
