using CatalogService.Api.Data;
using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Api.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.Images)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Include(x => x.Images)
            .Where(x => x.CategoryId == categoryId)
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        _dbContext.Products.Remove(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}