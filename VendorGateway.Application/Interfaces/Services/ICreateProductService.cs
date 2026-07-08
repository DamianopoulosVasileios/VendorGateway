namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateProductService
    {
        Task<bool> UpdateAsync(CancellationToken ct);
    }
}