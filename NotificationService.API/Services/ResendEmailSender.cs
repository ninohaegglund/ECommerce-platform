using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Options;

namespace NotificationService.Api.Services;

public class ResendEmailSender : IEmailSender
{
    private const string ProviderName = "Resend";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly IOptions<ResendOptions> _options;

    public ResendEmailSender(HttpClient httpClient, IOptions<ResendOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<EmailSendResult> SendAsync(EmailSendRequest request, CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        ValidateOptions(options);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildEndpoint(options));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            httpRequest.Headers.Add("Idempotency-Key", request.IdempotencyKey);
        }

        var payload = new ResendSendEmailRequest(
            FormatSender(options),
            [request.To],
            request.Subject,
            request.HtmlBody,
            request.TextBody);

        httpRequest.Content = JsonContent.Create(payload, options: JsonOptions);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Resend email send failed with status {(int)response.StatusCode}: {ExtractErrorMessage(responseBody)}");
        }

        var sendResponse = JsonSerializer.Deserialize<ResendSendEmailResponse>(responseBody, JsonOptions);
        return new EmailSendResult(ProviderName, sendResponse?.Id);
    }

    private static Uri BuildEndpoint(ResendOptions options)
    {
        var baseUri = new Uri(options.ApiBaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        return new Uri(baseUri, "emails");
    }

    private static void ValidateOptions(ResendOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException("Resend API key is missing. Set Resend:ApiKey with user-secrets or environment variables.");
        }

        if (string.IsNullOrWhiteSpace(options.FromEmail))
        {
            throw new InvalidOperationException("Resend sender email is missing. Set Resend:FromEmail to a verified sender address.");
        }
    }

    private static string FormatSender(ResendOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            return options.FromEmail;
        }

        return $"{options.FromName} <{options.FromEmail}>";
    }

    private static string ExtractErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return "No response body returned.";
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? responseBody;
            }
        }
        catch (JsonException)
        {
            return responseBody;
        }

        return responseBody;
    }

    private sealed record ResendSendEmailRequest(
        [property: JsonPropertyName("from")] string From,
        [property: JsonPropertyName("to")] IReadOnlyList<string> To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("html")] string? Html,
        [property: JsonPropertyName("text")] string Text);

    private sealed record ResendSendEmailResponse(
        [property: JsonPropertyName("id")] string? Id);
}
