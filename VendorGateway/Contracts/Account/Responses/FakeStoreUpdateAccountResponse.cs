namespace VendorGateway.Infrastructure.Contracts.Account.Responses
{
    public sealed record FakeStoreUpdateAccountResponse(int id, string username, string email, string password);
}
