using Microsoft.EntityFrameworkCore;
using VendorGateway.Application.Interfaces;
using VendorGateway.Application.Jobs.Entities;

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

        public DbSet<Application.Entities.Product> Products => Set<Application.Entities.Product>();
        public DbSet<Application.Entities.Account> Accounts => Set<Application.Entities.Account>();
        public DbSet<Application.Entities.Order> Orders => Set<Application.Entities.Order>();
        public DbSet<Application.Entities.OrderItem> OrderItems => Set<Application.Entities.OrderItem>();
        public DbSet<Job> Jobs => Set<Application.Jobs.Entities.Job>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Application.Entities.Order>()
                .HasIndex(o => o.IdempotencyKey)
                .IsUnique();

            modelBuilder.Entity<Application.Entities.Account>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Application.Entities.Order>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Application.Entities.Order>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Application.Entities.Product>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Application.Entities.OrderItem>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Application.Entities.Order>()
                .HasOne(o => o.Account)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.AccountId);

            modelBuilder.Entity<Application.Entities.OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<Application.Entities.OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<Job>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Job>()
                .Property(x => x.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<Job>()
                .HasIndex(x => new { x.Status, x.CreatedAt });

            modelBuilder.Entity<Job>()
                .Property(x => x.CreatedAt)
                .HasConversion(
                    v => v.UtcDateTime,
                    v => new DateTimeOffset(v, TimeSpan.Zero));

            modelBuilder.Entity<Job>()
                .Property(x => x.UpdatedAt)
                .HasConversion(
                    v => v.UtcDateTime,
                    v => new DateTimeOffset(v, TimeSpan.Zero));
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
