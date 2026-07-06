namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IProductQueries
    {
        Task<List<Entities.Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
        Task<List<Entities.Product>> GetAsync(CancellationToken ct);
    }
}