namespace VendorGateway.Application.Interfaces
{
    public interface IOrderCommands
    {
        Task CreateAsync(int accountId, List<Application.Dtos.OrderDetails.OrderItem> orderItem, CancellationToken ct);
        Task UpdateAsync(int accountId, Application.Dtos.OrderDetails.Order order, CancellationToken ct);
        Task DeleteByIdAsync(int accountId, int id, CancellationToken ct);
        Task DeleteAsync(int accountId, CancellationToken ct);
        Task ExecuteAsync(int accountId, int id, CancellationToken ct);
    }
}