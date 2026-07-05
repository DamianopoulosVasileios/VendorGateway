namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IProductCommands
    {
        Task<bool> AddRangeAsync(IEnumerable<Application.Entities.Product> products, CancellationToken ct);
        Task DeleteAsync(CancellationToken ct);
    }
}