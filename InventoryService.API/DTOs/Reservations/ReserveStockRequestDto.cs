using System.ComponentModel.DataAnnotations;

namespace InventoryService.Api.DTOs.Reservations;

public class ReserveStockRequestDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
