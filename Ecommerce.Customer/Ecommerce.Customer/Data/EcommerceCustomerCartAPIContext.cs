using Microsoft.EntityFrameworkCore;
using Ecommerce.Customer.CartAPI.Models;

namespace Ecommerce.Customer.CartAPI.Data
{
    public class EcommerceCustomerCartAPIContext : DbContext
    {
        public EcommerceCustomerCartAPIContext(DbContextOptions<EcommerceCustomerCartAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // One cart per user
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Cart → CartItems (1-to-many)
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // precision for money
            modelBuilder.Entity<CartItem>()
                .Property(c => c.Price)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Cart>()
                .Property(c => c.TotalAmount)
                .HasColumnType("decimal(10,2)");
        }
    }
}