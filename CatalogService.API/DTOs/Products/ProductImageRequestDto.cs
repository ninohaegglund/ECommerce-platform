namespace CatalogService.Api.DTOs.Products;

public class ProductImageRequestDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}