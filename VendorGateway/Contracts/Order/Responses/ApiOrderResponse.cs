namespace VendorGateway.Contracts.Order.Responses
{
    public sealed record ApiOrderResponse(
        int id,
        string FirstName,
        string LastName,
        string Email,
        DateTime CreatedAt);
}
