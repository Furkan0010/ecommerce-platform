using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence;

public class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(b =>
        {
            b.Property(o => o.BuyerId).IsRequired();
            b.HasMany(o => o.Items).WithOne(i => i.Order!).HasForeignKey(i => i.OrderId);
        });

        modelBuilder.Entity<OrderItem>(b =>
        {
            b.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            b.Property(i => i.ProductName).IsRequired().HasMaxLength(150);
        });

        // MassTransit Outbox tablolarını modele ekliyoruz (güvenli olay yayını için).
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
