using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Order
{
    public class OrderCommands : IOrderCommands
    {
        private readonly AppDbContext _db;
        private readonly IDbExceptionClassifier _dbExceptionClassifier;

        public OrderCommands(AppDbContext db, IDbExceptionClassifier dbExceptionClassifier)
        {
            _db = db;
            _dbExceptionClassifier = dbExceptionClassifier;
        }

        public async Task CreateAsync(int accountId, Guid idempotencyKey, List<Application.Dtos.OrderDetails.OrderItem> orderItem, CancellationToken ct)
        {
            var existing = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.IdempotencyKey == idempotencyKey, ct);

            if (existing is not null)
                return;

            var items = orderItem.Select(Application.Mappers.OrderMapper.ToDto).ToList();
            var orderToPersist = Application.Entities.Order.Create(accountId, idempotencyKey, items);

            try
            {
                await _db.Orders.AddAsync(orderToPersist, ct);
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (_dbExceptionClassifier.IsUniqueConstraintViolation(ex))
            {
                var winningOrder = await _db.Orders
                    .Include(o => o.Items)
                    .FirstAsync(o => o.IdempotencyKey == idempotencyKey, ct);
                //Log the exception and the winning order details for debugging purposes
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to persist order to database.", ex);
            }
        }

        public async Task UpdateAsync(int accountId, Application.Dtos.OrderDetails.Order order, CancellationToken ct)
        {
            var items = order.Items.Select(Application.Mappers.OrderMapper.ToDto).ToList();
            var newOrder = Application.Entities.Order.CreateWithOrderId(order.Id, accountId, items);

            try
            {
                var entity = await _db.Orders
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == newOrder.Id, ct);

                if (entity is null)
                    throw new KeyNotFoundException($"OrderId {order.Id} was not found for accountId {accountId}.");

                entity.UpdatePropertiesForOrderUpdate(newOrder);

                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to update order {order.Id}.", ex);
            }
        }

        public async Task DeleteByIdAsync(int accountId, int id, CancellationToken ct)
        {
            try
            {
                var deleted = await _db.Orders
                    .Where(x => x.AccountId == accountId && x.Id == id && x.Status != Application.Enums.OrderStatus.Submitted)
                    .ExecuteDeleteAsync(ct);

                if (deleted > 0)
                    return;

                var exists = await _db.Orders
                    .AnyAsync(x => x.AccountId == accountId && x.Id == id, ct);

                if (!exists)
                    throw new KeyNotFoundException($"OrderId {id} was not found for accountId {accountId}.");

                throw new InvalidOperationException(
                    $"OrderId {id} is already submitted for accountId {accountId}. Cannot delete submitted orders.");
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to delete order {id}.", ex);
            }
        }

        public async Task DeleteAsync(int accountId, CancellationToken ct)
        {
            try
            {
                var deleted = await _db.Orders
                    .Where(x => x.AccountId == accountId)
                    .ExecuteDeleteAsync(ct);

                if (deleted == 0)
                    throw new KeyNotFoundException($"Orders was not found for accountId {accountId}.");
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to delete orders.", ex);
            }
        }

        public async Task ExecuteAsync(int accountId, int id, CancellationToken ct)
        {
            try
            {
                var entity = await _db.Orders
                    .FirstOrDefaultAsync(x => x.AccountId == accountId && x.Id == id);

                if (entity is null)
                    throw new KeyNotFoundException($"OrderId {id} was not found for accountId {accountId}.");

                if (entity.Status == Application.Enums.OrderStatus.Submitted)
                {
                    throw new KeyNotFoundException($"OrderId {id} status is already submitted for accountId {accountId}.");
                }
                entity.ExecuteOrder();

                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to delete orders.", ex);
            }
        }
    }
}
