using Microsoft.EntityFrameworkCore;

namespace VendorGateway.Application.Interfaces
{
    public interface IDbExceptionClassifier
    {
        bool IsUniqueConstraintViolation(DbUpdateException ex);
    }
}
