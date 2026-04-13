using CatalogService.Api.Models;

namespace CatalogService.Api.DTOs.Products;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;

    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string Currency { get; set; } = "SEK";

    public int StockQuantity { get; set; }
    public bool IsActive { get; set; }
    public ProductStatus Status { get; set; }

    public List<ProductImageResponseDto> Images { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}