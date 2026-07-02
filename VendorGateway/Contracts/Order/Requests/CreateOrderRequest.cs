namespace VendorGateway.Contracts.Order.Requests
{
    public sealed record CreateOrderRequest(
        string FirstName,
        string LastName,
        string Email);
}
