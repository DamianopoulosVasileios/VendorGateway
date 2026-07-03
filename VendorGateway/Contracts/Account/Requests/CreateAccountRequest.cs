namespace VendorGateway.Contracts.Account.Requests
{
    public sealed record CreateAccountRequest(
        int id,
        string username,
        string email,
        string password);
}