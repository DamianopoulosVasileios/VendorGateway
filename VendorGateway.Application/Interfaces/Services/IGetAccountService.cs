namespace VendorGateway.Application.Interfaces.Services
{
    public interface IGetAccountService
    {
        Task<Entities.Account?> GetAsync(int accountId, CancellationToken ct);
    }
}