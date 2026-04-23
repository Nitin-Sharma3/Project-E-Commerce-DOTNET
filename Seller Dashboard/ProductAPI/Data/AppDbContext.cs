using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;

namespace ProductAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // Table name
            entity.ToTable("Products");

            // Primary Key
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id)
                  .ValueGeneratedOnAdd();

            // Name
            entity.Property(p => p.Name)
                  .IsRequired()
                  .HasMaxLength(150);

            // Price
            entity.Property(p => p.Price)
                  .IsRequired()
                  .HasPrecision(10, 2);

            // Stock
            entity.Property(p => p.Stock)
                  .IsRequired()
                  .HasDefaultValue(0);

            // Description
            entity.Property(p => p.Description)
                  .HasMaxLength(1000);

            // Image URLs
            entity.Property(p => p.ImageUrl1)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(p => p.ImageUrl2)
                  .HasMaxLength(500);

            entity.Property(p => p.ImageUrl3)
                  .HasMaxLength(500);

            // Index for faster product search
            entity.HasIndex(p => p.Name);
        });
    }
}