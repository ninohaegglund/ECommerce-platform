using System.Globalization;
using System.Text;
using NotificationService.Api.DTOs.Notifications;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public Task<NotificationLog> SendOrderConfirmationAsync(OrderConfirmationRequestDto request, CancellationToken cancellationToken = default)
    {
        var subject = $"Order Confirmation - #{request.OrderNumber}";
        var body = BuildOrderConfirmationBody(request);

        return SendAsync(
            request.UserId,
            request.OrderId,
            request.RecipientEmail,
            NotificationType.OrderConfirmation,
            subject,
            body,
            cancellationToken);
    }

    public Task<NotificationLog> SendPaymentConfirmationAsync(PaymentConfirmationRequestDto request, CancellationToken cancellationToken = default)
    {
        var subject = $"Payment Received for Order #{request.OrderNumber}";
        var body =
            $"Hello,\n\n" +
            $"We have received your payment of {request.Amount.ToString("0.00", CultureInfo.InvariantCulture)} {request.Currency} " +
            $"for order #{request.OrderNumber}.\n" +
            $"Transaction ID: {request.TransactionId}.\n\n" +
            $"Thank you for shopping with us.";

        return SendAsync(
            request.UserId,
            request.OrderId,
            request.RecipientEmail,
            NotificationType.PaymentConfirmation,
            subject,
            body,
            cancellationToken);
    }

    public Task<NotificationLog> SendPaymentFailedAsync(PaymentFailedRequestDto request, CancellationToken cancellationToken = default)
    {
        var subject = $"Payment Failed for Order #{request.OrderNumber}";
        var body =
            $"Hello,\n\n" +
            $"Your payment of {request.Amount.ToString("0.00", CultureInfo.InvariantCulture)} {request.Currency} " +
            $"for order #{request.OrderNumber} could not be processed.\n" +
            $"Reason: {request.FailureReason}\n\n" +
            $"Please update your payment method and try again.";

        return SendAsync(
            request.UserId,
            request.OrderId,
            request.RecipientEmail,
            NotificationType.PaymentFailed,
            subject,
            body,
            cancellationToken);
    }

    public Task<IReadOnlyList<NotificationLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => _notificationRepository.GetByUserIdAsync(userId, cancellationToken);

    private async Task<NotificationLog> SendAsync(
        Guid userId,
        Guid orderId,
        string recipientEmail,
        NotificationType type,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var notification = new NotificationLog
        {
            UserId = userId,
            OrderId = orderId,
            Type = type,
            RecipientEmail = recipientEmail,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending
        };

        _logger.LogInformation(
            "Notification {Type} to {Recipient} | Subject: {Subject}\n{Body}",
            type, recipientEmail, subject, body);

        notification.Status = NotificationStatus.Sent;
        notification.SentAtUtc = DateTime.UtcNow;

        return await _notificationRepository.AddAsync(notification, cancellationToken);
    }

    private static string BuildOrderConfirmationBody(OrderConfirmationRequestDto request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Hello,");
        builder.AppendLine();
        builder.AppendLine($"Thank you for your order #{request.OrderNumber}.");
        builder.AppendLine();

        if (request.Items.Count > 0)
        {
            builder.AppendLine("Items:");
            foreach (var item in request.Items)
            {
                var lineTotal = item.UnitPrice * item.Quantity;
                builder.AppendLine(
                    $"  - {item.ProductName} x {item.Quantity} @ " +
                    $"{item.UnitPrice.ToString("0.00", CultureInfo.InvariantCulture)} {request.Currency} " +
                    $"= {lineTotal.ToString("0.00", CultureInfo.InvariantCulture)} {request.Currency}");
            }
            builder.AppendLine();
        }

        builder.AppendLine($"Total: {request.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture)} {request.Currency}");
        return builder.ToString();
    }
}
