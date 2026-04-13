using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;

namespace CatalogService.Api.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => _productRepository.GetAllAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _productRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => _productRepository.GetByCategoryIdAsync(categoryId, cancellationToken);

    public Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.CreatedAtUtc = DateTime.UtcNow;
        product.UpdatedAtUtc = null;

        foreach (var image in product.Images)
        {
            image.ProductId = product.Id;
        }

        return _productRepository.AddAsync(product, cancellationToken);
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

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _productRepository.DeleteAsync(id, cancellationToken);
}