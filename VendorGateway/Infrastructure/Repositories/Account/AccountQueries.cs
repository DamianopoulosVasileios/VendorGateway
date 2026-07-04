using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Account
{
    public class AccountQueries : IAccountQueries
    {
        private readonly AppDbContext _db;

        public AccountQueries(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Entities.Account>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var data = await _db.Accounts
                .Where(p => ids.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync(ct);

            return data;
        }
    }
}
