using System.ComponentModel.DataAnnotations;

namespace InventoryService.Api.DTOs.StockItems;

public class SetStockRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityAvailable { get; set; }
}
