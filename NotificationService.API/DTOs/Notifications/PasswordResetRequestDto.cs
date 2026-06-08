using System.ComponentModel.DataAnnotations;

namespace NotificationService.Api.DTOs.Notifications;

public class PasswordResetRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string RecipientEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAtUtc { get; set; }

    [Url]
    [MaxLength(1000)]
    public string? ResetUrl { get; set; }
}
