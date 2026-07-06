namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IAccountQueries
    {
        Task<IReadOnlyList<Entities.Account>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct);
    }
}