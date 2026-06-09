using NotificationService.Api.DTOs.Notifications;
using NotificationService.Api.Models;

namespace NotificationService.Api.Interfaces;

public interface INotificationService
{
    Task<NotificationLog> SendAccountCreatedAsync(AccountCreatedRequestDto request, CancellationToken cancellationToken = default);
    Task<NotificationLog> SendEmailVerificationAsync(EmailVerificationRequestDto request, CancellationToken cancellationToken = default);
    Task<NotificationLog> SendPasswordResetAsync(PasswordResetRequestDto request, CancellationToken cancellationToken = default);
    Task<NotificationLog> SendOrderConfirmationAsync(OrderConfirmationRequestDto request, CancellationToken cancellationToken = default);
    Task<NotificationLog> SendPaymentConfirmationAsync(PaymentConfirmationRequestDto request, CancellationToken cancellationToken = default);
    Task<NotificationLog> SendPaymentFailedAsync(PaymentFailedRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
