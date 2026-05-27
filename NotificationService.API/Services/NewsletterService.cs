using NotificationService.Api.DTOs.Newsletter;
using NotificationService.Api.Interfaces;
using NotificationService.Api.Models;

namespace NotificationService.Api.Services;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterRepository _newsletterRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NewsletterService> _logger;

    public NewsletterService(
        INewsletterRepository newsletterRepository,
        INotificationRepository notificationRepository,
        IEmailSender emailSender,
        ILogger<NewsletterService> logger)
    {
        _newsletterRepository = newsletterRepository;
        _notificationRepository = notificationRepository;
        _emailSender = emailSender;
        _logger = logger;
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

    public async Task<NewsletterSendResponseDto> SendAsync(
        SendNewsletterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var subscribers = await _newsletterRepository.GetSubscribersAsync(includeUnsubscribed: false, cancellationToken);
        var response = new NewsletterSendResponseDto
        {
            TotalSubscribers = subscribers.Count
        };

        foreach (var subscriber in subscribers)
        {
            var notification = new NotificationLog
            {
                Type = NotificationType.Newsletter,
                RecipientEmail = subscriber.Email,
                Subject = request.Subject,
                Body = request.Body,
                Status = NotificationStatus.Pending
            };

            try
            {
                var sendResult = await _emailSender.SendAsync(
                    new EmailSendRequest(
                        subscriber.Email,
                        request.Subject,
                        BuildPersonalizedBody(request.Body, subscriber),
                        request.HtmlBody,
                        notification.Id.ToString("N")),
                    cancellationToken);

                notification.Provider = sendResult.Provider;
                notification.ProviderMessageId = sendResult.MessageId;
                notification.Status = NotificationStatus.Sent;
                notification.SentAtUtc = DateTime.UtcNow;
                response.SentCount++;
            }
            catch (Exception exception)
            {
                notification.Status = NotificationStatus.Failed;
                notification.FailureReason = LimitFailureReason(exception.Message);
                response.FailedCount++;

                _logger.LogWarning(
                    exception,
                    "Could not send newsletter email to {Email}.",
                    subscriber.Email);
            }

            await _notificationRepository.AddAsync(notification, cancellationToken);

            response.Recipients.Add(new NewsletterRecipientResultDto
            {
                Email = subscriber.Email,
                Status = notification.Status,
                ProviderMessageId = notification.ProviderMessageId,
                FailureReason = notification.FailureReason
            });
        }

        return response;
    }

    public async Task<NewsletterRecipientResultDto> SendTestAsync(
        SendNewsletterTestRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var notification = new NotificationLog
        {
            Type = NotificationType.Newsletter,
            RecipientEmail = request.RecipientEmail,
            Subject = request.Subject,
            Body = request.Body,
            Status = NotificationStatus.Pending
        };

        try
        {
            var sendResult = await _emailSender.SendAsync(
                new EmailSendRequest(
                    request.RecipientEmail,
                    request.Subject,
                    request.Body,
                    request.HtmlBody,
                    notification.Id.ToString("N")),
                cancellationToken);

            notification.Provider = sendResult.Provider;
            notification.ProviderMessageId = sendResult.MessageId;
            notification.Status = NotificationStatus.Sent;
            notification.SentAtUtc = DateTime.UtcNow;
        }
        catch (Exception exception)
        {
            notification.Status = NotificationStatus.Failed;
            notification.FailureReason = LimitFailureReason(exception.Message);

            _logger.LogWarning(
                exception,
                "Could not send newsletter test email to {Email}.",
                request.RecipientEmail);
        }

        await _notificationRepository.AddAsync(notification, cancellationToken);

        return new NewsletterRecipientResultDto
        {
            Email = request.RecipientEmail,
            Status = notification.Status,
            ProviderMessageId = notification.ProviderMessageId,
            FailureReason = notification.FailureReason
        };
    }

    private static string BuildPersonalizedBody(string body, NewsletterSubscriber subscriber)
    {
        var name = $"{subscriber.FirstName} {subscriber.LastName}".Trim();
        return string.IsNullOrWhiteSpace(name)
            ? body
            : $"Hello {name},\n\n{body}";
    }

    private static string LimitFailureReason(string message)
    {
        const int maxLength = 1000;
        return message.Length <= maxLength ? message : message[..maxLength];
    }
}
