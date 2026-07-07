namespace VendorGateway.Application.Interfaces.Services
{
    public interface IGetProductService
    {
        Task<List<Entities.Product>> GetAsync(CancellationToken ct);
    }
}