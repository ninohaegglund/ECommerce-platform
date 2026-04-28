using System.ComponentModel.DataAnnotations;

namespace NotificationService.Api.DTOs.Notifications;

public class OrderConfirmationRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string RecipientEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "SEK";

    public List<OrderItemDto> Items { get; set; } = [];
}
