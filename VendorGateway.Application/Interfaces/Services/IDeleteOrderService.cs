namespace VendorGateway.Application.Interfaces.Services
{
    public interface IDeleteOrderService
    {
        Task DeleteAsync(int accountId, int id, CancellationToken ct);
    }
}