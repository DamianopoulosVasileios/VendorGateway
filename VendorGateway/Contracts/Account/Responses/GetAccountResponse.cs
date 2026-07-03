namespace VendorGateway.Contracts.Account.Responses
{
    public sealed record GetAccountResponse(
        int id,
        string username,
        string email,
        string password);

}
