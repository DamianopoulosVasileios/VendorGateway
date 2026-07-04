namespace VendorGateway.Infrastructure.Contracts.Account.Requests
{
    public sealed record FakeStoreCreateAccountRequest(int id, string username, string email, string password);
}