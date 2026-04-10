using System;
using System.Collections.Generic;

namespace CatalogService.Api.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Currency { get; set; } = "SEK";

    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public List<ProductImage> Images { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}