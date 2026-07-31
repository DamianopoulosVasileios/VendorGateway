using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Jobs.Commands;
using VendorGateway.Application.Jobs.Entities;
using static VendorGateway.Application.Jobs.DTOs.AsynchronousAPI;

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

            try
            {
                switch (job.Type)
                {
                    case JobType.CreateAccount:
                        {
                            var payload = JsonSerializer.Deserialize<CreateAccountJobPayload>(job.Payload!)!;
                            await services.GetRequiredService<ICreateAccountService>()
                                .CreateAsync(payload.Request, payload.Id, ct);
                            break;
                        }
                    case JobType.UpdateAccount:
                        {
                            var payload = JsonSerializer.Deserialize<UpdateAccountJobPayload>(job.Payload!)!;
                            await services.GetRequiredService<IUpdateAccountService>()
                                .UpdateAsync(payload.Request, payload.Id, ct);
                            break;
                        }
                    case JobType.DeleteAccount:
                        {
                            var payload = JsonSerializer.Deserialize<DeleteAccountJobPayload>(job.Payload!)!;
                            await services.GetRequiredService<IDeleteAccountService>()
                                .DeleteAsync(payload.Id, ct);
                            break;
                        }
                    case JobType.CreateOrder:
                        {
                            var payload = JsonSerializer.Deserialize<CreateOrderJobPayload>(job.Payload!)!;
                            await services.GetRequiredService<ICreateOrderService>()
                                .CreateAsync(payload.AccountId, payload.IdempotencyKey, payload.Request, ct);
                            break;
                        }
                    case JobType.UpdateOrder:
                        {
                            var payload = JsonSerializer.Deserialize<UpdateOrderJobPayload>(job.Payload!)!;
                            await services.GetRequiredService<IUpdateOrderService>()
                                .UpdateAsync(payload.AccountId, payload.Id, payload.Request, ct);
                            break;
                        }
                    case JobType.DeleteOrder:
                        {
                            var payload = JsonSerializer.Deserialize<DeleteOrderJobPayload>(job.Payload!)!;
                            await services.GetRequiredService<IDeleteOrderService>()
                                .DeleteAsync(payload.AccountId, payload.Id, ct);
                            break;
                        }
                    case JobType.ExecuteOrder:
                        {
                            var payload = JsonSerializer.Deserialize<ExecuteOrderJobPayload>(job.Payload!)!;
                            await services.GetRequiredService<IExecuteOrderService>()
                                .ExecuteAsync(payload.AccountId, payload.Id, ct);
                            break;
                        }
                    case JobType.CreateProduct:
                        {
                            await services.GetRequiredService<ICreateProductService>().UpdateAsync(ct);
                            break;
                        }
                    case JobType.DeleteProduct:
                        {
                            await services.GetRequiredService<IDeleteProductService>().DeleteAsync(ct);
                            break;
                        }
                    default:
                        throw new InvalidOperationException($"Unhandled job type: {job.Type}");
                }

                await jobCommands.MarkCompletedAsync(job.Id, ct);
            }
            catch (Exception ex)
            {
                await jobCommands.MarkFailedAsync(job.Id, ex.Message, ct);
            }
        }
    }
}
