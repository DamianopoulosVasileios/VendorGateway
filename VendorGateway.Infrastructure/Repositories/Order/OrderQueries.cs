using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Dtos;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Application.Mappers;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Order
{
    public class OrderQueries : IOrderQueries
    {
        private readonly AppDbContext _db;

        public OrderQueries(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<OrderDetails.Order>> GetByIdsAsync(int accountId, IEnumerable<int> ids, CancellationToken ct)
        {
            var data = await _db.Orders
                .Where(p => p.AccountId == accountId && ids.Contains(p.Id))
                .Include(o => o.Items)
                .AsNoTracking()
                .ToListAsync(ct);

            return [.. data.Select(OrderMappers.ToDto)];
        }

        public async Task<List<OrderDetails.Order>> GetAsync(int accountId, CancellationToken ct)
        {
            var data = await _db.Orders
                .Where(p => p.AccountId == accountId)
                .Include(o => o.Items)
                .AsNoTracking()
                .ToListAsync(ct);

            return [.. data.Select(OrderMappers.ToDto)];
        }

        public async Task<List<OrderDetails.OrderItem>> GetOrderItemsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var data = await _db.OrderItems
                .Where(p => ids.Contains(p.OrderId))
                .AsNoTracking()
                .ToListAsync(ct);

            return [.. data.Select(OrderMappers.ToDto)];
        }
    }
}
