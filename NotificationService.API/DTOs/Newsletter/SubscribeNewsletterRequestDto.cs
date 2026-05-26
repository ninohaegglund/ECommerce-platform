using System.ComponentModel.DataAnnotations;

namespace NotificationService.Api.DTOs.Newsletter;

public class SubscribeNewsletterRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
