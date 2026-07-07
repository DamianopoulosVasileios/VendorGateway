using VendorGateway.Application.Enums;

namespace VendorGateway.Application.Dtos
{
    public class OrderResponse
    {
        public sealed record GetOrder(int Id, int AccountId, float TotalAmount, float TotalQuantity, OrderStatus Status, List<OrderItem> Items, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);
        public sealed record OrderItem(int ItemId, int ProductId, int Quantity, float UnitPrice);
    }
}
