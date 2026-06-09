namespace IdentityService.API.DTOs;

public class ConfirmEmailRequestDto
{
    public string Email { get; set; } = null!;
    public string Token { get; set; } = null!;
}
