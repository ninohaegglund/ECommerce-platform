using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;

namespace OrderService.Api.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly OrderDbContext _dbContext;

    public WishlistRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Wishlist?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Wishlists
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<Wishlist> AddAsync(Wishlist wishlist, CancellationToken cancellationToken = default)
    {
        _dbContext.Wishlists.Add(wishlist);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return wishlist;
    }

    public async Task AddItemAsync(WishlistItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.WishlistItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
