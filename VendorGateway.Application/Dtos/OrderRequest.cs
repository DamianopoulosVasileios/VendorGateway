namespace VendorGateway.Application.Dtos
{
    public class OrderRequest
    {
        public sealed record CreateOrder(IReadOnlyList<OrderItems> Items);
        public sealed record UpdateOrder(IReadOnlyList<OrderItems> Items);
        public sealed record OrderItems(int ProductId, int Quantity);
    }
}
