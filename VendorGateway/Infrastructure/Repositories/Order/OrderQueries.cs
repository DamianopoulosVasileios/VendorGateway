using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;
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

        public async Task<List<Entities.Order>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var data = await _db.Orders
                .Where(p => ids.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync(ct);

            return data;
        }
    }
}
