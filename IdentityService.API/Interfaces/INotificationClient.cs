namespace IdentityService.API.Interfaces;

public interface INotificationClient
{
    Task SendAccountCreatedAsync(AccountCreatedNotificationRequest request, CancellationToken cancellationToken = default);
    Task SendEmailVerificationAsync(EmailVerificationNotificationRequest request, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(PasswordResetNotificationRequest request, CancellationToken cancellationToken = default);
}

public sealed record AccountCreatedNotificationRequest(
    Guid UserId,
    string RecipientEmail,
    string FirstName,
    string LastName);

public sealed record EmailVerificationNotificationRequest(
    Guid UserId,
    string RecipientEmail,
    string FirstName,
    string LastName,
    string VerificationToken,
    DateTime ExpiresAtUtc,
    string? VerificationUrl);

public sealed record PasswordResetNotificationRequest(
    Guid UserId,
    string RecipientEmail,
    string FirstName,
    string LastName,
    string ResetToken,
    DateTime ExpiresAtUtc,
    string? ResetUrl);
