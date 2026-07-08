using VendorGateway.Application.Jobs.Entities;

namespace VendorGateway.Application.Jobs.Commands
{
    public interface IJobCommands
    {
        Task<List<Job>> ClaimNextBatchAsync(int batchSize, CancellationToken ct, JobStatus jobStatus = JobStatus.Pending);
        Task CreateAsync(Job job, CancellationToken ct);
        Task MarkCompletedAsync(Guid jobId, CancellationToken ct);
        Task MarkFailedAsync(Guid jobId, string errorMessage, CancellationToken ct);
    }
}
