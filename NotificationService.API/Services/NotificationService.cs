using System.Globalization;
using System.Text;
using NotificationService.Api.DTOs.Notifications;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IEmailSender emailSender,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _emailSender = emailSender;
        _logger = logger;
    }

    public Task<NotificationLog> SendAccountCreatedAsync(AccountCreatedRequestDto request, CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Spelvalvet";
        var body =
            $"Hello {FormatName(request.FirstName, request.LastName)},\n\n" +
            "Your Spelvalvet account has been created.\n\n" +
            "You can now sign in and start shopping.";

        return SendAsync(
            request.UserId,
            null,
            request.RecipientEmail,
            NotificationType.AccountCreated,
            subject,
            body,
            cancellationToken);
    }

    public Task<NotificationLog> SendEmailVerificationAsync(EmailVerificationRequestDto request, CancellationToken cancellationToken = default)
    {
        var subject = "Verify your Spelvalvet email";
        var body = BuildEmailVerificationBody(request);

        return SendAsync(
            request.UserId,
            null,
            request.RecipientEmail,
            NotificationType.EmailVerification,
            subject,
            body,
            cancellationToken,
            "Email verification instructions were sent.");
    }

    public Task<NotificationLog> SendPasswordResetAsync(PasswordResetRequestDto request, CancellationToken cancellationToken = default)
    {
        var subject = "Reset your Spelvalvet password";
        var body = BuildPasswordResetBody(request);

        return SendAsync(
            request.UserId,
            null,
            request.RecipientEmail,
            NotificationType.PasswordReset,
            subject,
            body,
            cancellationToken,
            "Password reset instructions were sent.");
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
        Guid? orderId,
        string recipientEmail,
        NotificationType type,
        string subject,
        string body,
        CancellationToken cancellationToken,
        string? logBody = null)
    {
        var notification = new NotificationLog
        {
            UserId = userId,
            OrderId = orderId,
            Type = type,
            RecipientEmail = recipientEmail,
            Subject = subject,
            Body = logBody ?? body,
            Status = NotificationStatus.Pending
        };

        _logger.LogInformation(
            "Sending notification {Type} to {Recipient} | Subject: {Subject}",
            type, recipientEmail, subject);

        try
        {
            var sendResult = await _emailSender.SendAsync(
                new EmailSendRequest(
                    recipientEmail,
                    subject,
                    body,
                    IdempotencyKey: notification.Id.ToString("N")),
                cancellationToken);

            notification.Provider = sendResult.Provider;
            notification.ProviderMessageId = sendResult.MessageId;
            notification.Status = NotificationStatus.Sent;
            notification.SentAtUtc = DateTime.UtcNow;
        }
        catch (Exception exception)
        {
            notification.Status = NotificationStatus.Failed;
            notification.FailureReason = LimitFailureReason(exception.Message);

            _logger.LogError(
                exception,
                "Failed to send notification {Type} to {Recipient} | Subject: {Subject}",
                type,
                recipientEmail,
                subject);
        }

        _logger.LogInformation(
            "Notification {Type} to {Recipient} finished with status {Status}",
            type,
            recipientEmail,
            notification.Status);

        return await _notificationRepository.AddAsync(notification, cancellationToken);
    }

    private static string LimitFailureReason(string message)
    {
        const int maxLength = 1000;
        return message.Length <= maxLength ? message : message[..maxLength];
    }

    private static string FormatName(string firstName, string lastName)
    {
        var fullName = $"{firstName} {lastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? "there" : fullName;
    }

    private static string BuildEmailVerificationBody(EmailVerificationRequestDto request)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Hello {FormatName(request.FirstName, request.LastName)},");
        builder.AppendLine();
        builder.AppendLine("Please verify your Spelvalvet email address before signing in.");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.VerificationUrl))
        {
            builder.AppendLine($"Verify your email: {request.VerificationUrl}");
            builder.AppendLine();
        }

        builder.AppendLine($"Verification code: {request.VerificationToken}");
        builder.AppendLine($"This code expires at {request.ExpiresAtUtc:yyyy-MM-dd HH:mm} UTC.");
        builder.AppendLine();
        builder.AppendLine("If you did not create this account, you can ignore this email.");
        return builder.ToString();
    }

    private static string BuildPasswordResetBody(PasswordResetRequestDto request)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Hello {FormatName(request.FirstName, request.LastName)},");
        builder.AppendLine();
        builder.AppendLine("We received a request to reset your Spelvalvet password.");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(request.ResetUrl))
        {
            builder.AppendLine($"Reset your password: {request.ResetUrl}");
            builder.AppendLine();
        }

        builder.AppendLine($"Reset code: {request.ResetToken}");
        builder.AppendLine($"This code expires at {request.ExpiresAtUtc:yyyy-MM-dd HH:mm} UTC.");
        builder.AppendLine();
        builder.AppendLine("If you did not request this reset, you can ignore this email.");
        return builder.ToString();
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
