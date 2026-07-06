namespace VendorGateway.Application.Interfaces.Services
{
    public interface IDeleteAccountService
    {
        Task DeleteAsync(int id, CancellationToken ct);
    }
}