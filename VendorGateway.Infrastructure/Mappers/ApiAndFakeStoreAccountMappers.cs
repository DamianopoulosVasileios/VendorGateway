using VendorGateway.Application.Dtos;
using VendorGateway.Infrastructure.Apis.Contracts.Requests;
using VendorGateway.Infrastructure.Apis.Contracts.Responses;

namespace VendorGateway.Infrastructure.Mappers
{
    public static class AccountMappers
    {
        public static GetAccountVendorResponse ToApi(FakeStoreGetAccountResponse response)
        {
            return new GetAccountVendorResponse(response.id, response.email);
        }
        public static CreateAccountVendorResponse ToApi(FakeStoreCreateAccountResponse request)
        {
            return new CreateAccountVendorResponse(request.id, request.email);
        }
        public static UpdateAccountVendorResponse ToApi(FakeStoreUpdateAccountResponse request)
        {
            return new UpdateAccountVendorResponse(request.id);
        }

        public static FakeStoreCreateAccountRequest ToFakeStore(CreateAccountRequest request, int id)
        {
            return new FakeStoreCreateAccountRequest(id, request.email);
        }
        public static FakeStoreUpdateAccountRequest ToFakeStore(UpdateAccountRequest request, int id)
        {
            return new FakeStoreUpdateAccountRequest(id);
        }
    }
}
