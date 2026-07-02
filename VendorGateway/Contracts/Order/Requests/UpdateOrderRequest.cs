namespace VendorGateway.Contracts.Order.Requests
{
    public sealed record UpdateOrderRequest(
        string FirstName,
        string LastName,
        string Email);
}
