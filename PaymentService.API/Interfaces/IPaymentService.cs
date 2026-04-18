using PaymentService.Api.DTOs;
using PaymentService.Api.Models;

namespace PaymentService.Api.Interfaces;

public interface IPaymentService
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> ProcessAsync(Guid id, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default);
}