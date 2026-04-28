using NotificationService.Api.Models;

namespace NotificationService.Api.DTOs.Notifications;

public class NotificationResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }

    public NotificationType Type { get; set; }

    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    public DateTime? SentAtUtc { get; set; }

    public NotificationStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
