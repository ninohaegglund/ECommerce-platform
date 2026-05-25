using System.Net.Http.Json;
using PaymentService.Api.Interfaces;

namespace PaymentService.Api.Services;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationClient> _logger;

    public NotificationClient(HttpClient httpClient, ILogger<NotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendPaymentConfirmationAsync(
        PaymentConfirmationNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_httpClient.BaseAddress is null)
            {
                _logger.LogWarning("NotificationServiceUrl is not configured. Skipping payment confirmation email.");
                return;
            }

            if (string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                _logger.LogWarning(
                    "Payment confirmation email skipped for payment transaction {TransactionId} because recipient email is missing.",
                    request.TransactionId);
                return;
            }

            using var response = await _httpClient.PostAsJsonAsync(
                "api/notifications/payment-confirmation",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "NotificationService rejected payment confirmation email for {Email}. Status: {StatusCode}. Response: {ResponseBody}",
                    request.RecipientEmail,
                    (int)response.StatusCode,
                    responseBody);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Could not send payment confirmation email for {Email}.",
                request.RecipientEmail);
        }
    }
}
