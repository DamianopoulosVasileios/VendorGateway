namespace VendorGateway.Contracts.Account.Requests
{
    public sealed record UpdateAccountRequest(
        int id,
        string username,
        string email,
        string password);
}
