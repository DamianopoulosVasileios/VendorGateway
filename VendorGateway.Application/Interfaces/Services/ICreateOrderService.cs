using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateOrderService
    {
        Task CreateAsync(Guid idempotencyKey, OrderRequest.CreateOrder request, CancellationToken ct);
    }
}