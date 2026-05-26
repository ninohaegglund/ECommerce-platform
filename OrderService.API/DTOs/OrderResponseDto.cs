using OrderService.Api.Models;

namespace OrderService.Api.DTOs;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? CustomerEmail { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public decimal SubtotalAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "SEK";

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public AddressResponseDto ShippingAddress { get; set; } = new();
    public AddressResponseDto BillingAddress { get; set; } = new();

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.NotPaid;
    public string? PaymentTransactionId { get; set; }
    public string? PaymentProvider { get; set; }

    public List<OrderItemResponseDto> Items { get; set; } = [];
}
