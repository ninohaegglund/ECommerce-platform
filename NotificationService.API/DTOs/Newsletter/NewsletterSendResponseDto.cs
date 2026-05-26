namespace NotificationService.Api.DTOs.Newsletter;

public class NewsletterSendResponseDto
{
    public int TotalSubscribers { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public List<NewsletterRecipientResultDto> Recipients { get; set; } = [];
}
