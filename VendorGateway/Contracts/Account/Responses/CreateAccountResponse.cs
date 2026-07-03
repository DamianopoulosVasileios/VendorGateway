namespace VendorGateway.Contracts.Account.Responses
{
    public sealed record CreateAccountResponse(
        int id,
        string username,
        string email,
        string password);
}
