using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Interfaces.CommandsQueries;
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

        public async Task UpdateAsync(int id, string email, CancellationToken ct)
        {
            var entity = await _db.Accounts
                .FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new KeyNotFoundException($"Account with id {id} was not found.");

            try
            {
                entity.Email = email;
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to update account {id}.", ex);
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
