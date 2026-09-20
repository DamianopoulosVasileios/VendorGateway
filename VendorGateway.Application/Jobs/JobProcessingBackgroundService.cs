using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VendorGateway.Application.Diagnostics;
using VendorGateway.Application.Jobs.Commands;
using VendorGateway.Application.Jobs.Entities;

namespace VendorGateway.Application.Jobs
{
    public class JobProcessingBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
        private const int BatchSize = 10;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Console.Error.WriteLine($"Job processing batch failed: {ex}");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task ProcessBatchAsync(CancellationToken ct)
        {
            using var scope = scopeFactory.CreateScope();
            var jobCommands = scope.ServiceProvider.GetRequiredService<IJobCommands>();

            var jobs = await jobCommands.ClaimNextBatchAsync(BatchSize, ct);

            foreach (var job in jobs)
                await ProcessJobAsync(job, ct);
        }

        private async Task ProcessJobAsync(Job job, CancellationToken ct)
        {
            using var scope = scopeFactory.CreateScope();
            var services = scope.ServiceProvider;
            var jobCommands = services.GetRequiredService<IJobCommands>();
            var metrics = services.GetRequiredService<VendorGatewayMetrics>();

            try
            {
                var result = await JobDispatcher.DispatchAsync(services, job, ct);
                if (result.IsSuccess)
                {
                    await jobCommands.MarkCompletedAsync(job.Id, ct);
                    metrics.JobCompleted(job.Type.ToString());
                }
                else
                {
                    await jobCommands.MarkFailedAsync(job.Id, result.Error!.Message, ct);
                    metrics.JobFailed(job.Type.ToString());
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await jobCommands.MarkFailedAsync(job.Id, ex.Message, ct);
                metrics.JobFailed(job.Type.ToString());
            }
        }
    }
}
