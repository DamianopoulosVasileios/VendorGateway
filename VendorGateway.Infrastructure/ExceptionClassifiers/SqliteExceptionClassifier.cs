using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.ExceptionClassifiers
{
    public sealed class SqliteExceptionClassifier : IDbExceptionClassifier
    {
        private const int SqliteConstraintViolation = 19; // SQLITE_CONSTRAINT

        public bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraintViolation };
    }
}
