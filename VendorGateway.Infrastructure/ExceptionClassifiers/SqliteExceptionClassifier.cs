using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.ExceptionClassifiers
{
    public sealed class SqliteExceptionClassifier : IDbExceptionClassifier
    {
        private const int SqliteConstraintViolation = 19; // SQLITE_CONSTRAINT
        private const int SqliteBusy = 5; // SQLITE_BUSY
        private const int SqliteLocked = 6; // SQLITE_LOCKED

        public bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is SqliteException { SqliteErrorCode: SqliteConstraintViolation };

        public bool IsTransientBusyError(DbUpdateException ex) =>
            ex.InnerException is SqliteException { SqliteErrorCode: SqliteBusy or SqliteLocked };
    }
}
