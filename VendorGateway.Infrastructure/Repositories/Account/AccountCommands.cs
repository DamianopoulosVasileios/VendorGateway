using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using VendorGateway.Application.Common;
using VendorGateway.Application.Interfaces.CommandsQueries;
using VendorGateway.Infrastructure.Dependencies;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Account
{
    public class AccountCommands : IAccountCommands
    {
        private readonly AppDbContext _db;
        private readonly IDbExceptionClassifier _dbExceptionClassifier;
        private readonly ResiliencePipeline _writePipeline;

        public AccountCommands(AppDbContext db, IDbExceptionClassifier dbExceptionClassifier, ResiliencePipelineProvider<string> pipelines)
        {
            _db = db;
            _dbExceptionClassifier = dbExceptionClassifier;
            _writePipeline = pipelines.GetPipeline(DependencyInjection.SqliteWriteResiliencePipeline);
        }

        public async Task<Result> CreateAsync(int id, string email, CancellationToken ct)
        {
            await _db.Accounts.AddAsync(new Application.Entities.Account { Id = id, Email = email }, ct);

            try
            {
                await _writePipeline.ExecuteAsync(async token => await _db.SaveChangesAsync(token), ct);
            }
            catch (DbUpdateException ex) when (_dbExceptionClassifier.IsUniqueConstraintViolation(ex))
            {
                return Result.Failure(Error.Conflict($"Account with id {id} already exists."));
            }

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(int id, string email, CancellationToken ct)
        {
            var entity = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
                return Result.Failure(Error.NotFound($"Account with id {id} was not found."));

            entity.Email = email;
            await _writePipeline.ExecuteAsync(async token => await _db.SaveChangesAsync(token), ct);

            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct)
        {
            var deleted = await _writePipeline.ExecuteAsync(
                async token => await _db.Accounts.Where(x => x.Id == id).ExecuteDeleteAsync(token),
                ct);

            if (deleted == 0)
                return Result.Failure(Error.NotFound($"Account with id {id} was not found."));

            return Result.Success();
        }
    }
}
