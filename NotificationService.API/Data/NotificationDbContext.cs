using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Models;

namespace NotificationService.Api.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RecipientEmail).HasMaxLength(320);
            entity.Property(x => x.Subject).HasMaxLength(300);
            entity.Property(x => x.Body).HasMaxLength(4000);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.OrderId);
        });
    }
}
