using VendorGateway.Contracts.Account.Requests;
using VendorGateway.Contracts.Account.Responses;
using VendorGateway.Infrastructure.Contracts.Account.Requests;
using VendorGateway.Infrastructure.Contracts.Account.Responses;

namespace VendorGateway.Mappers
{
    public static class ApiAndFakeStoreAccountMappers
    {
        public static ApiGetAccountResponse ToApi(FakeStoreGetAccountResponse response)
        {
            return new ApiGetAccountResponse(response.id, response.email);
        }
        public static ApiCreateAccountResponse ToApi(FakeStoreCreateAccountResponse request)
        {
            return new ApiCreateAccountResponse(request.id, request.email);
        }
        public static ApiUpdateAccountResponse ToApi(FakeStoreUpdateAccountResponse request)
        {
            return new ApiUpdateAccountResponse(request.id);
        }

        public static FakeStoreCreateAccountRequest ToFakeStore(ApiCreateAccountRequest request)
        {
            return new FakeStoreCreateAccountRequest(request.id, request.email);
        }
        public static FakeStoreUpdateAccountRequest ToFakeStore(ApiUpdateAccountRequest request)
        {
            return new FakeStoreUpdateAccountRequest(request.id);
        }
    }
}
