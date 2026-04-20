using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Customer.WishlistAPI.Models;

namespace Ecommerce.Customer.WishlistAPI.Data
{
    public class EcommerceCustomerWishlistAPIContext : DbContext
    {
        public EcommerceCustomerWishlistAPIContext (DbContextOptions<EcommerceCustomerWishlistAPIContext> options)
            : base(options)
        {
        }

        public DbSet<Ecommerce.Customer.WishlistAPI.Models.WishlistEntity> WishlistEntity { get; set; } = default!;
    }
}
