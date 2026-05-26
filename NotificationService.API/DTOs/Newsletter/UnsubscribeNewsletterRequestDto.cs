using System.ComponentModel.DataAnnotations;

namespace NotificationService.Api.DTOs.Newsletter;

public class UnsubscribeNewsletterRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;
}
