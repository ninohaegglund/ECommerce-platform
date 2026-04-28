namespace InventoryService.Api.Models;

public class StockReservation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }
    public Guid OrderId { get; set; }

    public int Quantity { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
