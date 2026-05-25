using System.Text.Json;

namespace OrderService.Api.DTOs;

public class UpdateOrderPaymentRequestDto
{
    public JsonElement? PaymentStatus { get; set; }
    public JsonElement? Status { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentProvider { get; set; }
    public string? Provider { get; set; }
    public string? OrderStatus { get; set; }
}
