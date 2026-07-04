namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IProductCommands
    {
        Task AddRangeAsync(IEnumerable<Entities.Product> products, CancellationToken ct);
    }
}