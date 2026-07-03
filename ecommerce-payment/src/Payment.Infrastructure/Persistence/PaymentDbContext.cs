using Microsoft.EntityFrameworkCore;
using PaymentEntity = Payment.Domain.Entities.Payment;

namespace Payment.Infrastructure.Persistence;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PaymentEntity>(b =>
        {
            b.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            b.Property(p => p.BuyerId).IsRequired();
            b.HasIndex(p => p.OrderNumber);
        });
    }
}
