using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface IUpdateAccountService
    {
        Task<Result> UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct);
    }
}
