using InventoryService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ProductId).IsUnique();
        });

        modelBuilder.Entity<StockReservation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.ProductId);
            entity.HasIndex(x => x.OrderId);
        });
    }
}
