namespace PaymentService.Api.DTOs;

public class OrderPaymentUpdateRequestDto
{
    public string PaymentStatus { get; set; } = string.Empty;
    public string? PaymentTransactionId { get; set; }
    public string? PaymentProvider { get; set; }
    public string? OrderStatus { get; set; }
}
