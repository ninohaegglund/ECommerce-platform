using System.ComponentModel.DataAnnotations;

namespace NotificationService.Api.DTOs.Newsletter;

public class SendNewsletterRequestDto
{
    [Required]
    [MaxLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? HtmlBody { get; set; }
}
