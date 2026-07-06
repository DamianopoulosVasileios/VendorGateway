using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IOrderQueries
    {
        Task<List<OrderDetails.Order>> GetByIdsAsync(int accountId, IEnumerable<int> ids, CancellationToken ct);
        Task<List<OrderDetails.Order>> GetAsync(int accountId, CancellationToken ct);
        Task<List<OrderDetails.OrderItem>> GetOrderItemsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}