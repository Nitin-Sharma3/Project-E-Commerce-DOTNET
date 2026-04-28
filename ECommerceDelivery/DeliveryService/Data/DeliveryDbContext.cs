using DeliveryService.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace DeliveryService.Data
{
    public class DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
     : DbContext(options)
    {
        public DbSet<Delivery> Deliveries => Set<Delivery>();
        public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();
        public DbSet<DeliveryStatusHistory> StatusHistories => Set<DeliveryStatusHistory>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Delivery>(e =>
            {
                e.HasIndex(d => d.TrackingId).IsUnique();
                e.HasIndex(d => d.OrderId).IsUnique();
                e.Property(d => d.Status).HasConversion<string>();
                e.Property(d => d.Pincode).HasMaxLength(20);
                e.Property(d => d.RecipientName).HasMaxLength(100);
                e.Property(d => d.ContactPhone).HasMaxLength(20);
                e.Property(d => d.TrackingId).HasMaxLength(50);
            });

            b.Entity<DeliveryStatusHistory>(e =>
            {
                e.Property(h => h.Status).HasConversion<string>();
            });

            b.Entity<DeliveryItem>(e =>
            {
                e.Property(i => i.UnitPrice).HasPrecision(18, 2);
                e.Property(i => i.Subtotal).HasPrecision(18, 2);
            });
        }
    }
}
