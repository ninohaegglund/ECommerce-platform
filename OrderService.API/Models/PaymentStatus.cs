namespace OrderService.Api.Models;

public enum PaymentStatus
{
    NotPaid = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3
}