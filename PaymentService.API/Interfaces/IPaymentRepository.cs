using PaymentService.Api.Models;

namespace PaymentService.Api.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment> UpdateAsync(Payment payment, CancellationToken cancellationToken = default);
}