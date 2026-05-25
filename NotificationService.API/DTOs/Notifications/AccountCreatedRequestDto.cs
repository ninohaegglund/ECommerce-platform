using System.ComponentModel.DataAnnotations;

namespace NotificationService.Api.DTOs.Notifications;

public class AccountCreatedRequestDto
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
}
