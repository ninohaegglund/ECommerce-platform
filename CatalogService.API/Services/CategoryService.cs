using CatalogService.Api.Interfaces;
using CatalogService.Api.Models;

namespace CatalogService.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
        => _categoryRepository.GetAllAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _categoryRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Category?> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        if (category.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(category.ParentCategoryId.Value, cancellationToken);
            if (parent is null)
            {
                return null;
            }
        }

        category.CreatedAtUtc = DateTime.UtcNow;
        category.UpdatedAtUtc = null;
        return await _categoryRepository.AddAsync(category, cancellationToken);
    }

    public async Task<Category?> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        var existing = await _categoryRepository.GetByIdAsync(category.Id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Name = category.Name;
        existing.Slug = category.Slug;
        existing.Description = category.Description;
        existing.IsActive = category.IsActive;
        existing.ParentCategoryId = category.ParentCategoryId;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        return await _categoryRepository.UpdateAsync(existing, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _categoryRepository.DeleteAsync(id, cancellationToken);
}