using PaymentService.Api.DTOs;
using PaymentService.Api.Interfaces;
using PaymentService.Api.Models;

namespace PaymentService.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _paymentRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Payment> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        payment.Status = PaymentStatus.Pending;
        payment.CreatedAtUtc = DateTime.UtcNow;
        payment.UpdatedAtUtc = null;
        payment.ProcessedAtUtc = null;

        return await _paymentRepository.AddAsync(payment, cancellationToken);
    }

    public async Task<Payment?> ProcessAsync(Guid id, ProcessPaymentRequestDto request, CancellationToken cancellationToken = default)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);
        if (payment is null)
        {
            return null;
        }

        if (payment.Status is PaymentStatus.Captured or PaymentStatus.Failed or PaymentStatus.Cancelled)
        {
            throw new InvalidOperationException("Payment is already finalized.");
        }

        payment.Status = request.IsSuccessful ? PaymentStatus.Captured : PaymentStatus.Failed;
        payment.TransactionId = request.TransactionId;
        payment.FailureReason = request.IsSuccessful ? null : request.FailureReason;
        payment.ProcessedAtUtc = DateTime.UtcNow;
        payment.UpdatedAtUtc = DateTime.UtcNow;

        return await _paymentRepository.UpdateAsync(payment, cancellationToken);
    }
}