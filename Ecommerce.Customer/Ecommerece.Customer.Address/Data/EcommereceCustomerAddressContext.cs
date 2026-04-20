using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ecommerece.Customer.Address.Models;

namespace Ecommerece.Customer.Address.Data
{
    public class EcommereceCustomerAddressContext : DbContext
    {
        public EcommereceCustomerAddressContext (DbContextOptions<EcommereceCustomerAddressContext> options)
            : base(options)
        {
        }

        public DbSet<Ecommerece.Customer.Address.Models.AddressEntity> Addresses { get; set; } = default!;
    }
}
