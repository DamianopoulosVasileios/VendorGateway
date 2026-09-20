using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IUpdateOrderService
    {
        Task<Result> UpdateAsync(int accountId, int id, OrderRequest.UpdateOrder request, CancellationToken ct);
    }
}
