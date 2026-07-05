namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IAccountQueries
    {
        Task<IReadOnlyList<Application.Entities.Account>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}