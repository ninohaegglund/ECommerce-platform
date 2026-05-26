using NotificationService.Api.Models;

namespace NotificationService.Api.DTOs.Newsletter;

public class NewsletterRecipientResultDto
{
    public string Email { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? FailureReason { get; set; }
}
