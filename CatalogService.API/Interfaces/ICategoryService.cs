using CatalogService.Api.Models;

namespace CatalogService.Api.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> CreateAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category?> UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}