using CatalogService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).HasMaxLength(150);
            entity.Property(x => x.Slug).HasMaxLength(150);
            entity.Property(x => x.Description).HasMaxLength(2000);

            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Products)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).HasMaxLength(200);
            entity.Property(x => x.Slug).HasMaxLength(200);
            entity.Property(x => x.Sku).HasMaxLength(100);
            entity.Property(x => x.ShortDescription).HasMaxLength(500);
            entity.Property(x => x.Description).HasMaxLength(4000);
            entity.Property(x => x.Currency).HasMaxLength(3);

            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.CompareAtPrice).HasPrecision(18, 2);

            entity.HasIndex(x => x.Slug).IsUnique();
            entity.HasIndex(x => x.Sku).IsUnique();

            entity.HasMany(x => x.Images)
                .WithOne()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ImageUrl).HasMaxLength(1000);
            entity.Property(x => x.AltText).HasMaxLength(300);
        });
    }
}