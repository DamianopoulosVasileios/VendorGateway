using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Interfaces.CommandsQueries;
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

        public async Task<IReadOnlyList<Application.Entities.Account>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var idList = ids as IList<int> ?? ids.ToList();

            if (idList.Count == 0)
                return [];

            var data = await _db.Accounts
                .Where(p => idList.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync(ct);

            return data;
        }
    }
}
