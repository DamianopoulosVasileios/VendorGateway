using VendorGateway.Application.Interfaces;

namespace VendorGateway.Application.Jobs.Entities
{
    public enum JobType
    {
        CreateAccount,
        UpdateAccount,
        DeleteAccount,

        CreateOrder,
        UpdateOrder,
        DeleteOrder,
        ExecuteOrder,

        CreateProduct,
        DeleteProduct
    }

    public enum JobStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    public class Job : IAuditable
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public JobType Type { get; set; }
        public string? Payload { get; set; }
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public string? ErrorMessage { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
