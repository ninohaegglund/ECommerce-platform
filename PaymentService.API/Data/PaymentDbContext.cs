using Microsoft.EntityFrameworkCore;
using PaymentService.Api.Models;

namespace PaymentService.Api.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Provider).HasMaxLength(100);
            entity.Property(x => x.TransactionId).HasMaxLength(200);
            entity.Property(x => x.FailureReason).HasMaxLength(1000);

            entity.Property(x => x.Amount).HasPrecision(18, 2);

            entity.HasIndex(x => x.OrderId);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.Status);
        });
    }
}