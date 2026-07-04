namespace VendorGateway.Infrastructure.Contracts.Account.Requests
{
    public sealed record FakeStoreUpdateAccountRequest(int id, string username, string email, string password);
}
