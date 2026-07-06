using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Interfaces.ApiClient
{
    public interface IAccountsApiClient
    {
        Task<GetAccountVendorResponse> GetByIdAsync(int id, CancellationToken ct);
        Task<CreateAccountVendorResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct);
        Task<UpdateAccountVendorResponse> UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct);
        Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken ct);
    }
}