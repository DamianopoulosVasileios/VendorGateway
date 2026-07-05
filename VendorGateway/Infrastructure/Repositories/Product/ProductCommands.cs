using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Interfaces;
using VendorGateway.Infrastructure.Persistence;

namespace VendorGateway.Infrastructure.Repositories.Product
{
    public class ProductCommands : IProductCommands
    {
        private readonly AppDbContext _db;
        private readonly IDbExceptionClassifier _dbExceptionClassifier;

        public ProductCommands(AppDbContext db, IDbExceptionClassifier dbExceptionClassifier)
        {
            _db = db;
            _dbExceptionClassifier = dbExceptionClassifier;
        }

        public async Task<bool> AddRangeAsync(IEnumerable<Entities.Product> products, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(products);

            var productList = products as IList<Entities.Product> ?? products.ToList();

            if (productList.Count == 0)
                return false;

            if (productList.Any(p => p is null))
                throw new ArgumentException("The batch must not contain null products.", nameof(products));

            var duplicateIds = productList
                .GroupBy(p => p.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIds.Count > 0)
                throw new ArgumentException(
                    $"The batch contains duplicate product ids: {string.Join(", ", duplicateIds)}.",
                    nameof(products));

            try
            {
                await _db.Products.AddRangeAsync(productList, ct);
                await _db.SaveChangesAsync(ct);
                return true;
            }
            catch (DbUpdateException ex) when (_dbExceptionClassifier.IsUniqueConstraintViolation(ex))
            {
                throw new InvalidOperationException("One or more products in the batch already exist.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to persist products to database.", ex);
            }
        }

        public async Task DeleteAsync(CancellationToken ct)
        {
            try
            {
                _db.Products.RemoveRange(_db.Products);
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to persist products to database.", ex);
            }
        }
    }
}
