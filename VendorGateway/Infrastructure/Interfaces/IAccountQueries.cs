namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IAccountQueries
    {
        Task<List<Entities.Account>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}