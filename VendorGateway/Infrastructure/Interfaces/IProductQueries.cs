namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IProductQueries
    {
        Task<List<Entities.Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}