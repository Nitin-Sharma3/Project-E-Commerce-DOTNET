using Microsoft.EntityFrameworkCore;
using RazorpayApi.Models;

namespace RazorpayApi.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<RefundRequestRecord> RefundRequests => Set<RefundRequestRecord>();
}
