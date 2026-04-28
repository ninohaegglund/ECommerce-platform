using InventoryService.Api.Interfaces;
using InventoryService.Api.Models;

namespace InventoryService.Api.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
        => _inventoryRepository.GetStockItemByProductIdAsync(productId, cancellationToken);

    public Task<StockReservation?> GetReservationByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
        => _inventoryRepository.GetReservationByIdReadOnlyAsync(reservationId, cancellationToken);

    public async Task<StockReservation> ReserveAsync(Guid productId, Guid orderId, int quantity, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("Quantity must be positive.");
        }

        var stock = await _inventoryRepository.GetStockItemForUpdateAsync(productId, cancellationToken);
        if (stock is null)
        {
            throw new InvalidOperationException($"No stock record exists for product {productId}.");
        }

        if (stock.QuantityAvailable < quantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. Requested {quantity}, available {stock.QuantityAvailable}.");
        }

        stock.QuantityAvailable -= quantity;
        stock.QuantityReserved += quantity;
        stock.UpdatedAtUtc = DateTime.UtcNow;

        var reservation = new StockReservation
        {
            ProductId = productId,
            OrderId = orderId,
            Quantity = quantity,
            Status = ReservationStatus.Pending
        };

        await _inventoryRepository.AddReservationAndSaveAsync(reservation, cancellationToken);
        return reservation;
    }

    public async Task<StockReservation?> ConfirmReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _inventoryRepository.GetReservationByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return null;
        }

        if (reservation.Status == ReservationStatus.Confirmed)
        {
            return reservation;
        }

        if (reservation.Status == ReservationStatus.Released)
        {
            throw new InvalidOperationException("Cannot confirm a released reservation.");
        }

        reservation.Status = ReservationStatus.Confirmed;
        reservation.UpdatedAtUtc = DateTime.UtcNow;

        await _inventoryRepository.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public async Task<StockReservation?> ReleaseReservationAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _inventoryRepository.GetReservationByIdAsync(reservationId, cancellationToken);
        if (reservation is null)
        {
            return null;
        }

        if (reservation.Status == ReservationStatus.Released)
        {
            return reservation;
        }

        var stock = await _inventoryRepository.GetStockItemForUpdateAsync(reservation.ProductId, cancellationToken);
        if (stock is null)
        {
            throw new InvalidOperationException($"Stock record missing for product {reservation.ProductId}.");
        }

        stock.QuantityReserved -= reservation.Quantity;
        stock.QuantityAvailable += reservation.Quantity;
        stock.UpdatedAtUtc = DateTime.UtcNow;

        reservation.Status = ReservationStatus.Released;
        reservation.UpdatedAtUtc = DateTime.UtcNow;

        await _inventoryRepository.SaveChangesAsync(cancellationToken);
        return reservation;
    }

    public Task<StockItem> SetStockAsync(Guid productId, int quantityAvailable, CancellationToken cancellationToken = default)
    {
        if (quantityAvailable < 0)
        {
            throw new InvalidOperationException("QuantityAvailable cannot be negative.");
        }

        return _inventoryRepository.UpsertStockItemAsync(productId, quantityAvailable, cancellationToken);
    }
}
