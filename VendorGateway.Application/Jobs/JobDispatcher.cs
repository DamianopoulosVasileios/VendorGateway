using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.Services;
using VendorGateway.Application.Jobs.Entities;
using static VendorGateway.Application.Jobs.DTOs.AsynchronousAPI;

namespace VendorGateway.Application.Jobs
{
    internal static class JobDispatcher
    {
        public static async Task<Result> DispatchAsync(IServiceProvider services, Job job, CancellationToken ct)
        {
            switch (job.Type)
            {
                case JobType.CreateAccount:
                    {
                        var payload = JsonSerializer.Deserialize<CreateAccountJobPayload>(job.Payload!)!;
                        return await services.GetRequiredService<ICreateAccountService>()
                            .CreateAsync(payload.Request, payload.Id, ct);
                    }
                case JobType.UpdateAccount:
                    {
                        var payload = JsonSerializer.Deserialize<UpdateAccountJobPayload>(job.Payload!)!;
                        return await services.GetRequiredService<IUpdateAccountService>()
                            .UpdateAsync(payload.Request, payload.Id, ct);
                    }
                case JobType.DeleteAccount:
                    {
                        var payload = JsonSerializer.Deserialize<DeleteAccountJobPayload>(job.Payload!)!;
                        return await services.GetRequiredService<IDeleteAccountService>()
                            .DeleteAsync(payload.Id, ct);
                    }
                case JobType.CreateOrder:
                    {
                        var payload = JsonSerializer.Deserialize<CreateOrderJobPayload>(job.Payload!)!;
                        return await services.GetRequiredService<ICreateOrderService>()
                            .CreateAsync(payload.AccountId, payload.IdempotencyKey, payload.Request, ct);
                    }
                case JobType.UpdateOrder:
                    {
                        var payload = JsonSerializer.Deserialize<UpdateOrderJobPayload>(job.Payload!)!;
                        return await services.GetRequiredService<IUpdateOrderService>()
                            .UpdateAsync(payload.AccountId, payload.Id, payload.Request, ct);
                    }
                case JobType.DeleteOrder:
                    {
                        var payload = JsonSerializer.Deserialize<DeleteOrderJobPayload>(job.Payload!)!;
                        return await services.GetRequiredService<IDeleteOrderService>()
                            .DeleteAsync(payload.AccountId, payload.Id, ct);
                    }
                case JobType.ExecuteOrder:
                    {
                        var payload = JsonSerializer.Deserialize<ExecuteOrderJobPayload>(job.Payload!)!;
                        return await services.GetRequiredService<IExecuteOrderService>()
                            .ExecuteAsync(payload.AccountId, payload.Id, ct);
                    }
                case JobType.CreateProduct:
                    return await services.GetRequiredService<ICreateProductService>().UpdateAsync(ct);
                case JobType.DeleteProduct:
                    return await services.GetRequiredService<IDeleteProductService>().DeleteAsync(ct);
                default:
                    return Result.Failure(Error.Unexpected($"Unhandled job type: {job.Type}"));
            }
        }
    }
}
