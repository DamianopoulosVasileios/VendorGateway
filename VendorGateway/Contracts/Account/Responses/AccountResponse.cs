namespace VendorGateway.Contracts.Account.Responses
{
    public sealed record AccountResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        DateTime CreatedAt);
}
