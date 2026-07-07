using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IUpdateOrderService
    {
        Task UpdateAsync(int accountId, int id, OrderRequest.UpdateOrder request, CancellationToken ct);
    }
}