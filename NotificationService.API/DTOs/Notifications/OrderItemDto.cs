using System.ComponentModel.DataAnnotations;

namespace NotificationService.Api.DTOs.Notifications;

public class OrderItemDto
{
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
