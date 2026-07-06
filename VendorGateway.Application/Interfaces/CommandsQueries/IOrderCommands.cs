namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IOrderCommands
    {
        Task CreateAsync(int accountId, Guid idempotencyKey, List<Dtos.OrderDetails.OrderItem> orderItem, CancellationToken ct);
        Task UpdateAsync(int accountId, Dtos.OrderDetails.Order order, CancellationToken ct);
        Task DeleteByIdAsync(int accountId, int id, CancellationToken ct);
        Task DeleteAsync(int accountId, CancellationToken ct);
        Task ExecuteAsync(int accountId, int id, CancellationToken ct);
    }
}