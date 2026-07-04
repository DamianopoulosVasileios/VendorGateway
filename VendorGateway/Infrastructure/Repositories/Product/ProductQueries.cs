using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Product
{
    public class ProductQueries : IProductQueries
    {
        private readonly AppDbContext _db;

        public ProductQueries(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Entities.Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var data = await _db.Products
                .Where(p => ids.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync(ct);

            return data;
        }
    }
}
