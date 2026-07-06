namespace VendorGateway.Infrastructure.Apis.Contracts.Responses
{
    public sealed record FakeStoreCreateAccountResponse(int id, string username, string email, string password);
}
