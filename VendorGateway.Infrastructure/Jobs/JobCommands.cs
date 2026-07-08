using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Jobs.Commands;
using VendorGateway.Application.Jobs.Entities;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Jobs.Commands
{
    public class JobCommands(AppDbContext db) : IJobCommands
    {
        public async Task CreateAsync(Job job, CancellationToken ct)
        {
            try
            {
                await db.Jobs.AddAsync(job, ct);
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to persist job to database.", ex);
            }
        }

        public async Task<List<Job>> ClaimNextBatchAsync(int batchSize, CancellationToken ct, JobStatus jobStatus = JobStatus.Pending)
        {
            var claimedIds = await db.Jobs
                .Where(j => j.Status == jobStatus)
                .OrderBy(j => j.CreatedAt)
                .Take(batchSize)
                .Select(j => j.Id)
                .ToListAsync(ct);

            if (claimedIds.Count == 0)
                return [];

            await db.Jobs
                .Where(j => claimedIds.Contains(j.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Processing), ct);

            return await db.Jobs
                .Where(j => claimedIds.Contains(j.Id))
                .ToListAsync(ct);
        }

        public async Task MarkCompletedAsync(Guid jobId, CancellationToken ct)
        {
            await db.Jobs
                .Where(j => j.Id == jobId)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, JobStatus.Completed), ct);
        }

        public async Task MarkFailedAsync(Guid jobId, string errorMessage, CancellationToken ct)
        {
            await db.Jobs
                .Where(j => j.Id == jobId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(j => j.Status, JobStatus.Failed)
                    .SetProperty(j => j.ErrorMessage, errorMessage), ct);
        }
    }
}
