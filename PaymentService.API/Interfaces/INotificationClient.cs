namespace PaymentService.Api.Interfaces;

public interface INotificationClient
{
    Task SendPaymentConfirmationAsync(PaymentConfirmationNotificationRequest request, CancellationToken cancellationToken = default);
}

public sealed record PaymentConfirmationNotificationRequest(
    Guid UserId,
    Guid OrderId,
    string OrderNumber,
    string RecipientEmail,
    decimal Amount,
    string Currency,
    string TransactionId);
