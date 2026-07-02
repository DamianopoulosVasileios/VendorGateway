namespace VendorGateway.Contracts.Order.Responses
{
    public sealed record OrderResponse(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        DateTime CreatedAt);
}
