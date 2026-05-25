using PaymentService.Api.DTOs;
using PaymentService.Api.Models;

namespace PaymentService.Api.Interfaces;

public interface IPaymentService
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> ProcessAsync(Guid id, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default);
    Task<StripePaymentIntentResponseDto?> CreateStripePaymentIntentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment?> ApplyStripePaymentIntentStatusAsync(string paymentIntentId, PaymentStatus status, string? failureReason, CancellationToken cancellationToken = default);
}
