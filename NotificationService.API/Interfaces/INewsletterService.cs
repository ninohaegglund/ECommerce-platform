using NotificationService.Api.DTOs.Newsletter;
using NotificationService.Api.Models;

namespace NotificationService.Api.Interfaces;

public interface INewsletterService
{
    Task<NewsletterSubscriber> SubscribeAsync(SubscribeNewsletterRequestDto request, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriber?> UnsubscribeAsync(UnsubscribeNewsletterRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsletterSubscriber>> GetSubscribersAsync(bool includeUnsubscribed, CancellationToken cancellationToken = default);
    Task<NewsletterSendResponseDto> SendAsync(SendNewsletterRequestDto request, CancellationToken cancellationToken = default);
    Task<NewsletterRecipientResultDto> SendTestAsync(SendNewsletterTestRequestDto request, CancellationToken cancellationToken = default);
}
