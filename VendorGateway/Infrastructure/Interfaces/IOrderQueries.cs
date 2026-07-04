namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IOrderQueries
    {
        Task<List<Entities.Order>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}