using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VendorGateway.API.Contracts.Order.Requests
{
    public sealed class ApiCreateOrderRequest
    {
        [Required, MinLength(1, ErrorMessage = "An order must contain at least one item.")]
        public IReadOnlyList<ApiOrderItemRequest> Items { get; init; } = [];

        public class ApiOrderItemRequest
        {
            [Required]
            public int ProductId { get; set; }
            [Required]
            [DefaultValue(1)]
            [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
            public int Quantity { get; set; }
        }
    }
}
