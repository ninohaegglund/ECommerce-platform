using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;

namespace CatalogService.Api.DTOs.Products;

public class ProductImageRequestDto
{
    [Required]
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class ProductImageUploadRequestDto
{
    [Required]
    public IFormFile Image { get; set; } = default!;

    [MaxLength(300)]
    public string? AltText { get; set; }

    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }
}