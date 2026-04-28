using InventoryService.Api.Data;
using InventoryService.Api.Interfaces;
using InventoryService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Api.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockItem?> GetStockItemByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
    }

    public async Task<StockItem?> GetStockItemForUpdateAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockItems
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
    }

    public async Task<StockItem> UpsertStockItemAsync(Guid productId, int quantityAvailable, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.StockItems
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);

        if (existing is null)
        {
            var stockItem = new StockItem
            {
                ProductId = productId,
                QuantityAvailable = quantityAvailable,
                QuantityReserved = 0
            };

            _dbContext.StockItems.Add(stockItem);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return stockItem;
        }

        existing.QuantityAvailable = quantityAvailable;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<StockReservation?> GetReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockReservations
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
    }

    public async Task<StockReservation?> GetReservationByIdReadOnlyAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockReservations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reservationId, cancellationToken);
    }

    public async Task AddReservationAndSaveAsync(StockReservation reservation, CancellationToken cancellationToken = default)
    {
        _dbContext.StockReservations.Add(reservation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
