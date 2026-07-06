using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.Services
{
    public interface ICreateAccountService
    {
        Task CreateAsync(CreateAccountRequest request, CancellationToken ct);
    }
}