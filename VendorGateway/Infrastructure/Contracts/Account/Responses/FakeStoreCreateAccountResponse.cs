namespace VendorGateway.API.Infrastructure.Contracts.Account.Responses
{
    public sealed record FakeStoreCreateAccountResponse(int id, string username, string email, string password);
}
