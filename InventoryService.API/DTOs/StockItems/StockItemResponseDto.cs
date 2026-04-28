namespace InventoryService.Api.DTOs.StockItems;

public class StockItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int QuantityAvailable { get; set; }
    public int QuantityReserved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
