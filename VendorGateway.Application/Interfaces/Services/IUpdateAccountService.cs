using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IUpdateAccountService
    {
        Task UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct);
    }
}