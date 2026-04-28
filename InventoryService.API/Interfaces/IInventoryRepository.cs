using InventoryService.Api.Models;

namespace InventoryService.Api.Interfaces;

public interface IInventoryRepository
{
    Task<StockItem?> GetStockItemByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<StockItem?> GetStockItemForUpdateAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<StockItem> UpsertStockItemAsync(Guid productId, int quantityAvailable, CancellationToken cancellationToken = default);

    Task<StockReservation?> GetReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<StockReservation?> GetReservationByIdReadOnlyAsync(Guid reservationId, CancellationToken cancellationToken = default);

    Task AddReservationAndSaveAsync(StockReservation reservation, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
