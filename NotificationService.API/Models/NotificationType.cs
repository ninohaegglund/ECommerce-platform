namespace NotificationService.Api.Models;

public enum NotificationType
{
    OrderConfirmation = 0,
    PaymentConfirmation = 1,
    PaymentFailed = 2,
    AccountCreated = 3,
    Newsletter = 4,
    EmailVerification = 5,
    PasswordReset = 6
}
