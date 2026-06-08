using IdentityService.API.DTOs;

namespace IdentityService.API.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task ConfirmEmailAsync(ConfirmEmailRequestDto request);
    Task ResendEmailVerificationAsync(ResendEmailVerificationRequestDto request);
    Task ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}
