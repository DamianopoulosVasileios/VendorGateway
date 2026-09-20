using VendorGateway.Application.Common;

namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IOrderCommands
    {
        Task<Result> CreateAsync(int accountId, Guid idempotencyKey, List<Dtos.OrderDetails.OrderItem> orderItem, CancellationToken ct);
        Task<Result> UpdateAsync(int accountId, Dtos.OrderDetails.Order order, CancellationToken ct);
        Task<Result> DeleteByIdAsync(int accountId, int id, CancellationToken ct);
        Task<Result> DeleteAsync(int accountId, CancellationToken ct);
        Task<Result> ExecuteAsync(int accountId, int id, CancellationToken ct);
    }
}
