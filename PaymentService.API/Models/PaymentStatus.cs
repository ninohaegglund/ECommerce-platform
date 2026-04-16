namespace PaymentService.Api.Models;

public enum PaymentStatus
{
    Pending = 0,
    Captured = 1,
    Failed = 2,
    Cancelled = 3,
    Refunded = 4
}