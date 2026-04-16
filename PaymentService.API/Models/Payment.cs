namespace PaymentService.Api.Models;

public enum PaymentStatus
{
    Pending = 0,
    Captured = 1,
    Failed = 2,
    Cancelled = 3,
    Refunded = 4
}

public enum PaymentMethod
{
    Card = 0,
    Swish = 1,
    Klarna = 2,
    PayPal = 3
}

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SEK";

    public PaymentMethod Method { get; set; } = PaymentMethod.Card;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string Provider { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}