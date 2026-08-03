using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateOrderService
    {
        Task<Result> CreateAsync(int accountId, Guid idempotencyKey, OrderRequest.CreateOrder request, CancellationToken ct);
    }
}
