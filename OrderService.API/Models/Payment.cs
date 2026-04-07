using System;

namespace OrderService.Api.Models;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PaymentMethod Method { get; set; } = PaymentMethod.CreditCard;
    public PaymentStatus Status { get; set; } = PaymentStatus.NotPaid;

    public string Provider { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string Currency { get; set; } = "SEK";
    public decimal Amount { get; set; }

    public DateTime? AuthorizedAtUtc { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
}