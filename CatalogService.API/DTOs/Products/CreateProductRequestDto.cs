using System.ComponentModel.DataAnnotations;
using CatalogService.Api.Models;

namespace CatalogService.Api.DTOs.Products;

public class CreateProductRequestDto
{
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Sku { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string ShortDescription { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "SEK";

    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    public List<ProductImageRequestDto> Images { get; set; } = [];
}