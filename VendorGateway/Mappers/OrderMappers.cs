using VendorGateway.Application.Dtos;
using VendorGateway.Contracts.Order.Requests;
using static VendorGateway.Application.Dtos.OrderRequest;

namespace VendorGateway.Mappers
{
    public static class OrderMappers
    {
        public static CreateOrder ToDto(this ApiCreateOrderRequest request)
        {
            return new CreateOrder(request.AccountId, request.Items.Select(i => new OrderItems(i.ProductId, i.Quantity)).ToList());
        }

        public static UpdateOrder ToDto(this ApiUpdateOrderRequest request)
        {
            return new UpdateOrder(request.AccountId, request.Items.Select(i => new OrderItems(i.ProductId, i.Quantity)).ToList());
        }

        public static OrderResponse.GetOrder ToApiResponse(this OrderDetails.Order order)
        {
            return new OrderResponse.GetOrder(order.Id, order.AccountId, order.TotalAmount, order.TotalQuantity, order.Status, order.Items.Select(i => new OrderResponse.OrderItem(i.ItemId, i.ProductId, i.Quantity, i.UnitPrice)).ToList(), order.CreatedAt, order.UpdatedAt);
        }
        public static OrderResponse.OrderItem ToApiResponse(this OrderDetails.OrderItem orderItem)
        {
            return new OrderResponse.OrderItem(orderItem.ItemId, orderItem.ProductId, orderItem.Quantity, orderItem.UnitPrice);
        }
    }
}
