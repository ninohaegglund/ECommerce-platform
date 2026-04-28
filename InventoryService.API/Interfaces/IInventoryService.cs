using InventoryService.Api.Models;

namespace InventoryService.Api.Interfaces;

public interface IInventoryService
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<StockReservation?> GetReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default);

    Task<StockReservation> ReserveAsync(Guid productId, Guid orderId, int quantity, CancellationToken cancellationToken = default);
    Task<StockReservation?> ConfirmReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<StockReservation?> ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);

    Task<StockItem> SetStockAsync(Guid productId, int quantityAvailable, CancellationToken cancellationToken = default);
}
