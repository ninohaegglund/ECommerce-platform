namespace IdentityService.API.Interfaces;

public interface INotificationClient
{
    Task SendAccountCreatedAsync(AccountCreatedNotificationRequest request, CancellationToken cancellationToken = default);
}

public sealed record AccountCreatedNotificationRequest(
    Guid UserId,
    string RecipientEmail,
    string FirstName,
    string LastName);
