namespace NotificationService.Api.Models;

public class NotificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }

    public NotificationType Type { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public DateTime? SentAtUtc { get; set; }

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
