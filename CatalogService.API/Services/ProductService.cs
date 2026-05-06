using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;
using Microsoft.AspNetCore.Http;

namespace CatalogService.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _environment = environment;
        _configuration = configuration;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => _productRepository.GetAllAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _productRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => _productRepository.GetByCategoryIdAsync(categoryId, cancellationToken);

    public async Task<Product?> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);
        if (category is null)
        {
            return null;
        }

        product.CreatedAtUtc = DateTime.UtcNow;
        product.UpdatedAtUtc = null;

        foreach (var image in product.Images)
        {
            image.ProductId = product.Id;
        }

        return await _productRepository.AddAsync(product, cancellationToken);
    }

    public async Task<Product?> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        var existing = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.CategoryId = product.CategoryId;
        existing.Name = product.Name;
        existing.Slug = product.Slug;
        existing.Sku = product.Sku;
        existing.ShortDescription = product.ShortDescription;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.CompareAtPrice = product.CompareAtPrice;
        existing.Currency = product.Currency;
        existing.StockQuantity = product.StockQuantity;
        existing.IsActive = product.IsActive;
        existing.Status = product.Status;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        existing.Images.Clear();
        foreach (var image in product.Images)
        {
            existing.Images.Add(new ProductImage
            {
                Id = image.Id == Guid.Empty ? Guid.NewGuid() : image.Id,
                ProductId = existing.Id,
                ImageUrl = image.ImageUrl,
                AltText = image.AltText,
                SortOrder = image.SortOrder,
                IsPrimary = image.IsPrimary
            });
        }

        return await _productRepository.UpdateAsync(existing, cancellationToken);
    }

    public async Task<ProductImage?> AddImageAsync(
        Guid productId,
        IFormFile image,
        string altText,
        int sortOrder,
        bool isPrimary,
        CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        var configuredPath = _configuration.GetValue<string>("ProductImages:UploadPath");
        var uploadRoot = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(_environment.ContentRootPath, "Uploads", "Products")
            : configuredPath;
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var filePath = Path.Combine(uploadRoot, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream, cancellationToken);
        }

        var baseUrl = _configuration.GetValue<string>("ProductImages:PublicBaseUrl")?.TrimEnd('/');
        var relativePath = $"uploads/products/{fileName}";
        var imageUrl = string.IsNullOrWhiteSpace(baseUrl) ? $"/{relativePath}" : $"{baseUrl}/{relativePath}";

        var productImage = new ProductImage
        {
            ProductId = product.Id,
            ImageUrl = imageUrl,
            AltText = altText,
            SortOrder = sortOrder,
            IsPrimary = isPrimary
        };

        return await _productRepository.AddImageAsync(productImage, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _productRepository.DeleteAsync(id, cancellationToken);
}