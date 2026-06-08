namespace IdentityService.API.DTOs;

public class RegisterResponseDto
{
    public string Message { get; set; } = string.Empty;
    public bool EmailVerificationRequired { get; set; }
    public UserDto User { get; set; } = null!;
}
