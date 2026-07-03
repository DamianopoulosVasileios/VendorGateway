using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;

namespace VendorGateway.Interfaces
{
    public interface IUsersApiClient
    {
        Task<GetAccountResponse> GetAsync(int id, CancellationToken ct);
        Task<CreateAccountResponse> CreateAsync(CreateAccountRequest request, CancellationToken ct);
        Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken ct);
        Task<UpdateAccountResponse> UpdateAsync(UpdateAccountRequest request, int id, CancellationToken ct);
    }
}