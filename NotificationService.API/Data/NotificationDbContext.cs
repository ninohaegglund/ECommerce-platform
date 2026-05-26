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
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RecipientEmail).HasMaxLength(320);
            entity.Property(x => x.Subject).HasMaxLength(300);
            entity.Property(x => x.Body).HasMaxLength(4000);
            entity.Property(x => x.Provider).HasMaxLength(100);
            entity.Property(x => x.ProviderMessageId).HasMaxLength(200);
            entity.Property(x => x.FailureReason).HasMaxLength(1000);

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.OrderId);
        });

        modelBuilder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email).HasMaxLength(320);
            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);

            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.IsSubscribed);
        });
    }
}
