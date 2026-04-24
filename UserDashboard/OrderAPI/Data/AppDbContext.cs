using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;
using System.Reflection.Emit;
namespace OrderAPI.Data
{
    

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Order → OrderItems (one-to-many, cascade delete)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ShippingAddress stored as owned entity (same table as Orders)
            modelBuilder.Entity<Order>()
                .OwnsOne(o => o.ShippingAddress, sa =>
                {
                    sa.Property(x => x.FullName).HasMaxLength(100).IsRequired();
                    sa.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                    sa.Property(x => x.AddressLine1).HasMaxLength(200).IsRequired();
                    sa.Property(x => x.AddressLine2).HasMaxLength(200);
                    sa.Property(x => x.City).HasMaxLength(100).IsRequired();
                    sa.Property(x => x.State).HasMaxLength(100).IsRequired();
                    sa.Property(x => x.PostalCode).HasMaxLength(20).IsRequired();
                    sa.Property(x => x.Country).HasMaxLength(100).IsRequired();
                });

            // Money precision
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            // Enum stored as string for readability
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            // Index for fast user order lookup
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);
        }
    }
}
