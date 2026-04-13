using CatalogService.Api.DTOs.Products;
using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(cancellationToken);
        return Ok(products.Select(MapToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(MapToResponse(product));
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        var products = await _productService.GetByCategoryIdAsync(categoryId, cancellationToken);
        return Ok(products.Select(MapToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request, CancellationToken cancellationToken)
    {
        var product = MapFromCreateRequest(request);
        var created = await _productService.CreateAsync(product, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponse(created));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequestDto request, CancellationToken cancellationToken)
    {
        var product = MapFromUpdateRequest(id, request);
        var updated = await _productService.UpdateAsync(product, cancellationToken);
        return updated is null ? NotFound() : Ok(MapToResponse(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _productService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private static Product MapFromCreateRequest(CreateProductRequestDto request)
    {
        return new Product
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            Currency = request.Currency,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            Status = request.Status,
            Images = request.Images.Select(MapImageFromRequest).ToList()
        };
    }

    private static Product MapFromUpdateRequest(Guid id, UpdateProductRequestDto request)
    {
        return new Product
        {
            Id = id,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Sku = request.Sku,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            Currency = request.Currency,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            Status = request.Status,
            Images = request.Images.Select(MapImageFromRequest).ToList()
        };
    }

    private static ProductImage MapImageFromRequest(ProductImageRequestDto image)
    {
        return new ProductImage
        {
            ImageUrl = image.ImageUrl,
            AltText = image.AltText,
            SortOrder = image.SortOrder,
            IsPrimary = image.IsPrimary
        };
    }

    private static ProductResponseDto MapToResponse(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            Name = product.Name,
            Slug = product.Slug,
            Sku = product.Sku,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Price = product.Price,
            CompareAtPrice = product.CompareAtPrice,
            Currency = product.Currency,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            Status = product.Status,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc,
            Images = product.Images
                .OrderBy(x => x.SortOrder)
                .Select(x => new ProductImageResponseDto
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    AltText = x.AltText,
                    SortOrder = x.SortOrder,
                    IsPrimary = x.IsPrimary
                })
                .ToList()
        };
    }
}