using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Account
{
    public class AccountQueries(AppDbContext db) : IAccountQueries
    {
        public async Task<IReadOnlyList<Application.Entities.Account>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct)
        {
            var idList = ids as IList<int> ?? ids.ToList();

            if (idList.Count == 0)
                return [];

            var data = await db.Accounts
                .Where(p => idList.Contains(p.Id))
                .AsNoTracking()
                .ToListAsync(ct);

            return data;
        }
    }
}
