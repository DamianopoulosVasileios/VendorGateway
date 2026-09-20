using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using VendorGateway.Application.Common;
using VendorGateway.Application.Diagnostics;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.Dependencies;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Order
{
    public class OrderCommands : IOrderCommands
    {
        private readonly AppDbContext _db;
        private readonly IDbExceptionClassifier _dbExceptionClassifier;
        private readonly ILogger<OrderCommands> _logger;
        private readonly VendorGatewayMetrics _metrics;
        private readonly ResiliencePipeline _writePipeline;

        public OrderCommands(
            AppDbContext db,
            IDbExceptionClassifier dbExceptionClassifier,
            ILogger<OrderCommands> logger,
            VendorGatewayMetrics metrics,
            ResiliencePipelineProvider<string> pipelines)
        {
            _db = db;
            _dbExceptionClassifier = dbExceptionClassifier;
            _logger = logger;
            _metrics = metrics;
            _writePipeline = pipelines.GetPipeline(DependencyInjection.SqliteWriteResiliencePipeline);
        }

        public async Task<Result> CreateAsync(int accountId, Guid idempotencyKey, List<Application.Dtos.OrderDetails.OrderItem> orderItem, CancellationToken ct)
        {
            var existing = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, ct);

            if (existing is not null)
                return Result.Success();

            var items = orderItem.Select(Application.Mappers.OrderMappers.ToDto).ToList();
            var orderToPersist = Application.Entities.Order.Create(accountId, idempotencyKey, items);

            var validation = ValidateOrder(orderToPersist);
            if (validation is not null)
                return validation;

            await _db.Orders.AddAsync(orderToPersist, ct);

            try
            {
                await _writePipeline.ExecuteAsync(async token => await _db.SaveChangesAsync(token), ct);
                _metrics.OrderCreated();
            }
            catch (DbUpdateException ex) when (_dbExceptionClassifier.IsUniqueConstraintViolation(ex))
            {
                var winningOrder = await _db.Orders
                    .Include(o => o.Items)
                    .FirstAsync(o => o.IdempotencyKey == idempotencyKey, ct);

                _logger.LogWarning(ex,
                    "Concurrent CreateAsync for idempotency key {IdempotencyKey}: order {WinningOrderId} already persisted for account {AccountId}.",
                    idempotencyKey, winningOrder.Id, accountId);
            }

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(int accountId, Application.Dtos.OrderDetails.Order order, CancellationToken ct)
        {
            var items = order.Items.Select(Application.Mappers.OrderMappers.ToDto).ToList();
            var newOrder = Application.Entities.Order.CreateWithOrderId(order.Id, accountId, items);

            var validation = ValidateOrder(newOrder);
            if (validation is not null)
                return validation;

            var entity = await _db.Orders
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == newOrder.Id, ct);

            if (entity is null)
                return Result.Failure(Error.NotFound($"OrderId {order.Id} was not found for accountId {accountId}."));

            entity.UpdatePropertiesForOrderUpdate(newOrder);

            await _writePipeline.ExecuteAsync(async token => await _db.SaveChangesAsync(token), ct);

            return Result.Success();
        }

        public async Task<Result> DeleteByIdAsync(int accountId, int id, CancellationToken ct)
        {
            var deleted = await _writePipeline.ExecuteAsync(async token => await _db.Orders
                .Where(x => x.AccountId == accountId && x.Id == id && x.Status != Application.Enums.OrderStatus.Submitted)
                .ExecuteDeleteAsync(token), ct);

            if (deleted > 0)
                return Result.Success();

            var exists = await _db.Orders
                .AnyAsync(x => x.AccountId == accountId && x.Id == id, ct);

            if (!exists)
                return Result.Failure(Error.NotFound($"OrderId {id} was not found for accountId {accountId}."));

            return Result.Failure(Error.Conflict(
                $"OrderId {id} is already submitted for accountId {accountId}. Cannot delete submitted orders."));
        }

        public async Task<Result> DeleteAsync(int accountId, CancellationToken ct)
        {
            var deleted = await _writePipeline.ExecuteAsync(async token => await _db.Orders
                .Where(x => x.AccountId == accountId && x.Status != Application.Enums.OrderStatus.Submitted)
                .ExecuteDeleteAsync(token), ct);

            if (deleted > 0)
                return Result.Success();

            var exists = await _db.Orders
                .AnyAsync(x => x.AccountId == accountId, ct);

            if (!exists)
                return Result.Failure(Error.NotFound($"Orders was not found for accountId {accountId}."));

            // Every remaining order for this account is already submitted — nothing deletable, not an error.
            return Result.Success();
        }

        public async Task<Result> ExecuteAsync(int accountId, int id, CancellationToken ct)
        {
            var entity = await _db.Orders
                .FirstOrDefaultAsync(x => x.AccountId == accountId && x.Id == id, ct);

            if (entity is null)
                return Result.Failure(Error.NotFound($"OrderId {id} was not found for accountId {accountId}."));

            if (entity.Status == Application.Enums.OrderStatus.Submitted)
                return Result.Failure(Error.Conflict($"OrderId {id} is already submitted for accountId {accountId}."));

            entity.ExecuteOrder();

            await _writePipeline.ExecuteAsync(async token => await _db.SaveChangesAsync(token), ct);

            return Result.Success();
        }

        private static Result? ValidateOrder(Application.Entities.Order newOrder)
        {
            if (newOrder.TotalAmount == 0)
                return Result.Failure(Error.Validation("Order to be updated can not have 0 total ammount"));

            if (newOrder.TotalQuantity == 0)
                return Result.Failure(Error.Validation("Order to be updated can not have 0 total quantity"));

            return null;
        }
    }
}
