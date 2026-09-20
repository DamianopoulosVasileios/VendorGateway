using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IGetAccountService
    {
        Task<Result<Entities.Account>> GetAsync(int accountId, CancellationToken ct);
    }
}
