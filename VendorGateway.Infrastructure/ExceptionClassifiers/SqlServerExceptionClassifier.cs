using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.ExceptionClassifiers
{
    public sealed class SqlServerExceptionClassifier : IDbExceptionClassifier
    {
        public bool IsUniqueConstraintViolation(DbUpdateException ex) =>
            ex.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
