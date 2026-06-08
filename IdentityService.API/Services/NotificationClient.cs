using System.Net.Http.Json;
using IdentityService.API.Interfaces;

namespace IdentityService.API.Services;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationClient> _logger;

    public NotificationClient(HttpClient httpClient, ILogger<NotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendAccountCreatedAsync(AccountCreatedNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_httpClient.BaseAddress is null)
            {
                _logger.LogWarning("NotificationServiceUrl is not configured. Skipping account-created email.");
                return;
            }

            using var response = await _httpClient.PostAsJsonAsync(
                "api/notifications/account-created",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "NotificationService rejected account-created email for {Email}. Status: {StatusCode}. Response: {ResponseBody}",
                    request.RecipientEmail,
                    (int)response.StatusCode,
                    responseBody);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not send account-created email for {Email}.",
                request.RecipientEmail);
        }
    }

    public async Task SendEmailVerificationAsync(EmailVerificationNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_httpClient.BaseAddress is null)
            {
                _logger.LogWarning("NotificationServiceUrl is not configured. Skipping email verification email.");
                return;
            }

            using var response = await _httpClient.PostAsJsonAsync(
                "api/notifications/email-verification",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "NotificationService rejected email verification email for {Email}. Status: {StatusCode}. Response: {ResponseBody}",
                    request.RecipientEmail,
                    (int)response.StatusCode,
                    responseBody);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not send email verification email for {Email}.",
                request.RecipientEmail);
        }
    }

    public async Task SendPasswordResetAsync(PasswordResetNotificationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_httpClient.BaseAddress is null)
            {
                _logger.LogWarning("NotificationServiceUrl is not configured. Skipping password reset email.");
                return;
            }

            using var response = await _httpClient.PostAsJsonAsync(
                "api/notifications/password-reset",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "NotificationService rejected password reset email for {Email}. Status: {StatusCode}. Response: {ResponseBody}",
                    request.RecipientEmail,
                    (int)response.StatusCode,
                    responseBody);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not send password reset email for {Email}.",
                request.RecipientEmail);
        }
    }
}
