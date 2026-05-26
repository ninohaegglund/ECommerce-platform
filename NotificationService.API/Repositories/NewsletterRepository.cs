using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Data;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Repositories;

public class NewsletterRepository : INewsletterRepository
{
    private readonly NotificationDbContext _dbContext;

    public NewsletterRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        return _dbContext.NewsletterSubscribers
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);
    }

    public async Task<IReadOnlyList<NewsletterSubscriber>> GetSubscribersAsync(
        bool includeUnsubscribed,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.NewsletterSubscribers.AsNoTracking();
        if (!includeUnsubscribed)
        {
            query = query.Where(x => x.IsSubscribed);
        }

        return await query
            .OrderByDescending(x => x.SubscribedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<NewsletterSubscriber> AddAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default)
    {
        subscriber.Email = NormalizeEmail(subscriber.Email);
        _dbContext.NewsletterSubscribers.Add(subscriber);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return subscriber;
    }

    public async Task<NewsletterSubscriber> UpdateAsync(NewsletterSubscriber subscriber, CancellationToken cancellationToken = default)
    {
        subscriber.Email = NormalizeEmail(subscriber.Email);
        _dbContext.NewsletterSubscribers.Update(subscriber);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return subscriber;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
