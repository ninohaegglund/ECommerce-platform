using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Interfaces;
using OrderService.Api.Models;

namespace OrderService.Api.Repositories;

public class CartRepository : ICartRepository
{
    private readonly OrderDbContext _dbContext;

    public CartRepository(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<Cart> AddAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        _dbContext.Carts.Add(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task<Cart> UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return cart;
    }

    public async Task AddItemAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.CartItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveItemAsync(CartItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}