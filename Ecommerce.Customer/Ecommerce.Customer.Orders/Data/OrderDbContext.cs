using Ecommerce.Customer.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Ecommerce.Customer.OrderAPI.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderAddress> OrderAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(e =>
            {
                e.HasKey(o => o.Id);
                e.HasIndex(o => o.OrderNumber).IsUnique();
                e.HasIndex(o => o.UserId);
                e.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
                e.Property(o => o.DeliveryCharge).HasColumnType("decimal(18,2)");
                e.Property(o => o.Discount).HasColumnType("decimal(18,2)");
                e.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                e.HasMany(o => o.Items)
                 .WithOne(i => i.Order)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(o => o.DeliveryAddress)
                 .WithOne(a => a.Order)
                 .HasForeignKey<OrderAddress>(a => a.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(e =>
            {
                e.HasKey(i => i.Id);
                e.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
                e.Property(i => i.SubTotal).HasColumnType("decimal(18,2)");
            });
        }
    }
}