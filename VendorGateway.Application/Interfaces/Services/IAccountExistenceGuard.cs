namespace VendorGateway.Application.Interfaces.Services
{
    public interface IAccountExistenceGuard
    {
        Task EnsureExistsAsync(int accountId, CancellationToken ct);
    }
}
