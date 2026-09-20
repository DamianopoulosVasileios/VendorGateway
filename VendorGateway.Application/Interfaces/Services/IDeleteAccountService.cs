using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IDeleteAccountService
    {
        Task<Result> DeleteAsync(int id, CancellationToken ct);
    }
}
