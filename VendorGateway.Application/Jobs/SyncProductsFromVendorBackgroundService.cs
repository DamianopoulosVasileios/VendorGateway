using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VendorGateway.Application.Interfaces.Services;

namespace VendorGateway.Application.Jobs
{
    public class SyncProductsFromVendorBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(5);

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
            var services = scope.ServiceProvider;
            await services.GetRequiredService<ICreateProductService>().UpdateAsync(ct);
        }
    }
}