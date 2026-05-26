using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using OrderService.Api.Interfaces;

namespace OrderService.Api.Services;

public class IdentityUserClient : IIdentityUserClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<IdentityUserClient> _logger;

    public IdentityUserClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<IdentityUserClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) ||
            !AuthenticationHeaderValue.TryParse(authorization, out var authorizationHeader))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{userId}");
        request.Headers.Authorization = authorizationHeader;

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Identity user lookup failed for user {UserId} with status code {StatusCode}.",
                    userId,
                    response.StatusCode);

                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<IdentityUserResponseDto>(cancellationToken);
            return string.IsNullOrWhiteSpace(user?.Email) ? null : user.Email;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Identity user lookup timed out for user {UserId}.", userId);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Identity user lookup failed for user {UserId}.", userId);
            return null;
        }
    }

    private sealed class IdentityUserResponseDto
    {
        public string? Email { get; set; }
    }
}
