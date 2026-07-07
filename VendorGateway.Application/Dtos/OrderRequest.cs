namespace VendorGateway.Application.Dtos
{
    public class OrderRequest
    {
        public sealed record CreateOrder(int AccountId, IReadOnlyList<OrderItems> Items);
        public sealed record UpdateOrder(int AccountId, IReadOnlyList<OrderItems> Items);
        public sealed record OrderItems(int ProductId, int Quantity);
    }
}
