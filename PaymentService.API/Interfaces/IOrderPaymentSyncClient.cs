using PaymentService.Api.Models;

namespace PaymentService.Api.Interfaces;

public interface IOrderPaymentSyncClient
{
    Task SyncPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
}
