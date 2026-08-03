using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IAccountExistenceGuard
    {
        Task<Result> EnsureExistsAsync(int accountId, CancellationToken ct);
    }
}
