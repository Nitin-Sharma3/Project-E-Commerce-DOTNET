using ECommerceUserApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceUserApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
