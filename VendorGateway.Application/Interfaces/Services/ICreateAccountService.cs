using VendorGateway.Application.Common;
using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateAccountService
    {
        Task<Result> CreateAsync(CreateAccountRequest request, int id, CancellationToken ct);
    }
}
