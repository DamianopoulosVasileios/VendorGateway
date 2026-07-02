namespace VendorGateway.Contracts.Account.Requests
{
    public sealed record UpdateAccountRequest(
        string FirstName,
        string LastName,
        string Email);
}
