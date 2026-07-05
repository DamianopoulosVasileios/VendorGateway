namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IAccountQueries
    {
        Task<IReadOnlyList<Entities.Account>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}