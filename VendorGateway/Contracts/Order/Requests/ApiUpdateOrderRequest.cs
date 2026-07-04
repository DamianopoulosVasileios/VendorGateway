namespace VendorGateway.Contracts.Order.Requests
{
    public class ApiUpdateOrderRequest
    {
        public List<ApiOrderItemRequest> Items { get; set; } = [];

        public class ApiOrderItemRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }

}
