namespace VendorGateway.Application.Interfaces.CommandsQueries
{
    public interface IProductCommands
    {
        Task<bool> UpdateRangeAsync(IEnumerable<Entities.Product> products, CancellationToken ct);
        Task DeleteAsync(CancellationToken ct);
    }
}