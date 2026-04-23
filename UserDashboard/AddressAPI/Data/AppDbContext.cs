  using AddressAPI.Models;
  using Microsoft.EntityFrameworkCore;
  
namespace AddressAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Address>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.FullName).HasMaxLength(100).IsRequired();
                entity.Property(a => a.Phone).HasMaxLength(20).IsRequired();
                entity.Property(a => a.AddressLine1).HasMaxLength(200).IsRequired();
                entity.Property(a => a.AddressLine2).HasMaxLength(200);
                entity.Property(a => a.City).HasMaxLength(100).IsRequired();
                entity.Property(a => a.State).HasMaxLength(100).IsRequired();
                entity.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
                entity.Property(a => a.Country).HasMaxLength(100).IsRequired();

                // Fast lookup by user
                entity.HasIndex(a => a.UserId);

                // Enforce only one primary per user at DB level
                entity.HasIndex(a => new { a.UserId, a.IsPrimary })
                      .HasFilter("[IsPrimary] = 1")
                      .IsUnique();
            });
        }
    }
}
