using Microsoft.EntityFrameworkCore;

namespace VendorGateway.Infrastructure.Interfaces
{
    public interface IDbExceptionClassifier
    {
        bool IsUniqueConstraintViolation(DbUpdateException ex);
        bool IsTransientBusyError(DbUpdateException ex);
    }
}
