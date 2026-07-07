namespace VendorGateway.Application.Interfaces.Services
{
    public interface IExecuteOrderService
    {
        Task ExecuteAsync(int accountId, int id, CancellationToken ct);
    }
}