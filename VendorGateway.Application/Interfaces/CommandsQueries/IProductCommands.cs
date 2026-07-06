namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IProductCommands
    {
        Task<bool> AddRangeAsync(IEnumerable<Entities.Product> products, CancellationToken ct);
        Task DeleteAsync(CancellationToken ct);
    }
}