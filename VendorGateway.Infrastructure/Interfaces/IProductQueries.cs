namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IProductQueries
    {
        Task<List<Application.Entities.Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
        Task<List<Application.Entities.Product>> GetAsync(CancellationToken ct);
    }
}