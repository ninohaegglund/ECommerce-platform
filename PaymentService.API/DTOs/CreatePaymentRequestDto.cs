using PaymentService.Api.Models;

namespace PaymentService.Api.DTOs;

public class CreatePaymentRequestDto
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SEK";
    public PaymentMethod Method { get; set; } = PaymentMethod.Card;
    public string Provider { get; set; } = string.Empty;
}