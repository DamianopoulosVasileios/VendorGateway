using VendorGateway.Application.Dtos;

namespace VendorGateway.Application.Jobs.DTOs
{
    public class AsynchronousAPI
    {
        #region Account
        public sealed record CreateAccountJobPayload(int Id, CreateAccountRequest Request);
        public sealed record UpdateAccountJobPayload(int Id, UpdateAccountRequest Request);
        public sealed record DeleteAccountJobPayload(int Id);
        #endregion

        #region Order
        public sealed record CreateOrderJobPayload(int AccountId, Guid IdempotencyKey, OrderRequest.CreateOrder Request);
        public sealed record UpdateOrderJobPayload(int AccountId, int Id, OrderRequest.UpdateOrder Request);
        public sealed record DeleteOrderJobPayload(int AccountId, int Id);
        public sealed record ExecuteOrderJobPayload(int AccountId, int Id);
        #endregion
    }
}
