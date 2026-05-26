using NotificationService.Api.Models;

namespace NotificationService.Api.Interfaces;

public interface INewsletterRepository
{
    Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NewsletterSubscriber>> GetSubscribersAsync(bool includeUnsubscribed, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriber> AddAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default);
    Task<NewsletterSubscriber> UpdateAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default);
}
