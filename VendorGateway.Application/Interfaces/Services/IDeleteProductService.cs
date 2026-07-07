namespace VendorGateway.Application.Interfaces.Services
{
    public interface IDeleteProductService
    {
        Task DeleteAsync(CancellationToken ct);
    }
}