namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateProductService
    {
        Task<bool> CreateAsync(CancellationToken ct);
    }
}