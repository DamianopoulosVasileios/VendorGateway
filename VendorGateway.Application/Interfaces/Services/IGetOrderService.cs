using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IGetOrderService
    {
        Task<OrderDetails.Order> GetAsync(int accountId, int id, CancellationToken ct);
    }
}