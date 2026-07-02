namespace VendorGateway.Contracts.Account.Requests
{
    public sealed record CreateAccountRequest(
        string FirstName,
        string LastName,
        string Email);
}