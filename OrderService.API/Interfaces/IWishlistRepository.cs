using OrderService.Api.Models;

namespace OrderService.Api.Interfaces;

public interface IWishlistRepository
{
    Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wishlist> AddAsync(Wishlist wishlist, CancellationToken cancellationToken = default);
    Task AddItemAsync(WishlistItem item, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
