using VendorGateway.Infrastructure.Entities;

namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IOrderCommands
    {
        Task CreateAsync(int id, int accountId, List<OrderItem> orderItems, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
        Task UpdateAsync(Entities.Order order, CancellationToken ct);
    }
}