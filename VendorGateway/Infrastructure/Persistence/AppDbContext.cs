using Microsoft.EntityFrameworkCore;
using VendorGateway.Infrastructure.Entities;
using VendorGateway.Infrastructure.Interfaces;

namespace VendorGateway.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly TimeProvider _timeProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, TimeProvider timeProvider)
        : base(options)
        {
            _timeProvider = timeProvider;
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Account>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Order>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Account)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.AccountId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            ApplyAuditInfo();
            return await base.SaveChangesAsync(ct);
        }

        private void ApplyAuditInfo()
        {
            var now = _timeProvider.GetUtcNow();

            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Property(x => x.CreatedAt).IsModified = false;
                        entry.Entity.UpdatedAt = now;
                        break;
                }
            }
        }
    }
}
