namespace VendorGateway.Contracts.Order.Responses
{
    public sealed record OrderResponse(
        int id,
        string FirstName,
        string LastName,
        string Email,
        DateTime CreatedAt);
}
