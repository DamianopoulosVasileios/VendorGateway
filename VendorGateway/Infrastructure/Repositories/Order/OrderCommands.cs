using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Entities;
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

        public async Task CreateAsync(int id, int accountId, List<OrderItem> orderItems, CancellationToken ct)
        {
            try
            {
                await _db.Orders.AddAsync(new Entities.Order { Id = id, AccountId = accountId, Items = orderItems }, ct);
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (_dbExceptionClassifier.IsUniqueConstraintViolation(ex))
            {
                throw new InvalidOperationException($"order with id {id} already exists.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to persist order to database.", ex);
            }
        }

        public async Task UpdateAsync(Entities.Order order, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(order);

            var entity = await _db.Orders
                .FirstOrDefaultAsync(x => x.Id == order.Id, ct)
                ?? throw new KeyNotFoundException($"order with id {order.Id} was not found.");

            try
            {
                //TODO add

                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to update order {order.Id}.", ex);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            try
            {
                var deleted = await _db.Orders
                    .Where(x => x.Id == id)
                    .ExecuteDeleteAsync(ct);

                if (deleted == 0)
                    throw new KeyNotFoundException($"Order with id {id} was not found.");
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to delete order {id}.", ex);
            }
        }
    }
}
