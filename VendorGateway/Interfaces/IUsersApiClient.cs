using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;

namespace VendorGateway.Interfaces
{
    public interface IUsersApiClient
    {
        Task<ApiGetAccountResponse> GetAsync(int id, CancellationToken ct);
        Task<ApiCreateAccountResponse> CreateAsync(ApiCreateAccountRequest request, CancellationToken ct);
        Task<HttpResponseMessage> DeleteAsync(int id, CancellationToken ct);
        Task<ApiUpdateAccountResponse> UpdateAsync(ApiUpdateAccountRequest request, int id, CancellationToken ct);
    }
}