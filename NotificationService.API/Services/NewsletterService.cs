using NotificationService.Api.DTOs.Newsletter;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Services;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterRepository _newsletterRepository;

    public NewsletterService(INewsletterRepository newsletterRepository)
    {
        _newsletterRepository = newsletterRepository;
    }

    public async Task<NewsletterSubscriber> SubscribeAsync(
        SubscribeNewsletterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var existingSubscriber = await _newsletterRepository.GetByEmailAsync(request.Email, cancellationToken);
        var now = DateTime.UtcNow;

        if (existingSubscriber is null)
        {
            return await _newsletterRepository.AddAsync(
                new NewsletterSubscriber
                {
                    Email = request.Email,
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    IsSubscribed = true,
                    SubscribedAtUtc = now,
                    CreatedAtUtc = now
                },
                cancellationToken);
        }

        existingSubscriber.FirstName = request.FirstName.Trim();
        existingSubscriber.LastName = request.LastName.Trim();
        existingSubscriber.IsSubscribed = true;
        existingSubscriber.SubscribedAtUtc = now;
        existingSubscriber.UnsubscribedAtUtc = null;
        existingSubscriber.UpdatedAtUtc = now;

        return await _newsletterRepository.UpdateAsync(existingSubscriber, cancellationToken);
    }

    public async Task<NewsletterSubscriber?> UnsubscribeAsync(
        UnsubscribeNewsletterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var subscriber = await _newsletterRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (subscriber is null)
        {
            return null;
        }

        subscriber.IsSubscribed = false;
        subscriber.UnsubscribedAtUtc = DateTime.UtcNow;
        subscriber.UpdatedAtUtc = DateTime.UtcNow;

        return await _newsletterRepository.UpdateAsync(subscriber, cancellationToken);
    }

    public Task<IReadOnlyList<NewsletterSubscriber>> GetSubscribersAsync(
        bool includeUnsubscribed,
        CancellationToken cancellationToken = default)
        => _newsletterRepository.GetSubscribersAsync(includeUnsubscribed, cancellationToken);
}
