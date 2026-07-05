using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Account
{
    public class AccountCommands : IAccountCommands
    {
        private readonly AppDbContext _db;
        private readonly IDbExceptionClassifier _dbExceptionClassifier;

        public AccountCommands(AppDbContext db, IDbExceptionClassifier dbExceptionClassifier)
        {
            _db = db;
            _dbExceptionClassifier = dbExceptionClassifier;
        }

        public async Task CreateAsync(int id, string email, CancellationToken ct)
        {
            try
            {
                await _db.Accounts.AddAsync(new Application.Entities.Account { Id = id, Email = email }, ct);
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (_dbExceptionClassifier.IsUniqueConstraintViolation(ex))
            {
                throw new InvalidOperationException($"Account with id {id} already exists.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to persist account to database.", ex);
            }
        }

        public async Task UpdateAsync(Application.Entities.Account account, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(account);

            var entity = await _db.Accounts
                .FirstOrDefaultAsync(x => x.Id == account.Id, ct)
                ?? throw new KeyNotFoundException($"Account with id {account.Id} was not found.");

            try
            {
                //There is no need to update the Id as it is the primary key and should not change.
                //There is nothing to update in this case, but if there were other properties to update, you would do it here.
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to update account {account.Id}.", ex);
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            try
            {
                var deleted = await _db.Accounts
                    .Where(x => x.Id == id)
                    .ExecuteDeleteAsync(ct);

                if (deleted == 0)
                    throw new KeyNotFoundException($"Account with id {id} was not found.");
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to delete account {id}.", ex);
            }
        }
    }
}
