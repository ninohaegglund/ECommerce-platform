namespace NotificationService.Api.DTOs.Newsletter;

public class NewsletterSubscriberResponseDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsSubscribed { get; set; }
    public DateTime SubscribedAtUtc { get; set; }
    public DateTime? UnsubscribedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
