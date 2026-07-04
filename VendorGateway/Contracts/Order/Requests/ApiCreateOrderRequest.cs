namespace VendorGateway.Contracts.Order.Requests
{
    public class ApiCreateOrderRequest
    {
        public int AccountId { get; set; }
        public List<ApiOrderItem> Items { get; set; } = [];

        public class ApiOrderItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
