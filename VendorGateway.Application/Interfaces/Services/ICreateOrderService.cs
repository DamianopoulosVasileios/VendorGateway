using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateOrderService
    {
        Task CreateAsync(int accountId, Guid idempotencyKey, OrderRequest.CreateOrder request, CancellationToken ct);
    }
}