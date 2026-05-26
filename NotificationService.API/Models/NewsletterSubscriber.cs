namespace NotificationService.Api.Models;

public class NewsletterSubscriber
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public bool IsSubscribed { get; set; } = true;
    public DateTime SubscribedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UnsubscribedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
