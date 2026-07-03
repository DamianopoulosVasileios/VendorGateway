namespace VendorGateway.Contracts.Account.Responses
{
    public sealed record UpdateAccountResponse(
        int id,
        string username,
        string email,
        string password);
}
