using IdentityService.API.Data;
using IdentityService.API.DTOs;
using IdentityService.API.Interfaces;
using IdentityService.API.JWT;
using IdentityService.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.API.Services
{
    public class AuthService : IAuthService
    {
        private const int EmailVerificationTokenLifetimeHours = 24;
        private const int PasswordResetTokenLifetimeHours = 1;

        private readonly IUserRepository _userRepository;
        private readonly IdentityDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly INotificationClient _notificationClient;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IdentityDbContext context,
            IJwtTokenGenerator jwtTokenGenerator,
            INotificationClient notificationClient,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _notificationClient = notificationClient;
            _configuration = configuration;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (request.Password != request.ConfirmPassword)
                throw new Exception("Passwords do not match.");

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("User with this email already exists.");

            var customerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Customer");

            if (customerRole == null)
                throw new Exception("Customer role not found.");

            var verificationToken = GenerateToken();
            var verificationTokenExpiresAtUtc = DateTime.UtcNow.AddHours(EmailVerificationTokenLifetimeHours);

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                EmailConfirmed = false,
                EmailVerificationTokenHash = HashToken(verificationToken),
                EmailVerificationTokenExpiresAtUtc = verificationTokenExpiresAtUtc,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = customerRole.Id,
                Role = customerRole
            });

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            await _notificationClient.SendEmailVerificationAsync(
                new EmailVerificationNotificationRequest(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    verificationToken,
                    verificationTokenExpiresAtUtc,
                    BuildActionUrl("AuthLinks:EmailVerificationUrl", user.Email, verificationToken)));

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

            return new RegisterResponseDto
            {
                Message = "Account created. Please verify your email before signing in.",
                EmailVerificationRequired = true,
                User = MapUser(user, roles)
            };
        }

        public async Task ConfirmEmailAsync(ConfirmEmailRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
                throw new Exception("Invalid or expired email verification token.");

            if (user.EmailConfirmed)
                return;

            if (string.IsNullOrWhiteSpace(user.EmailVerificationTokenHash) ||
                user.EmailVerificationTokenExpiresAtUtc == null ||
                user.EmailVerificationTokenExpiresAtUtc < DateTime.UtcNow ||
                !TokenMatches(request.Token, user.EmailVerificationTokenHash))
            {
                throw new Exception("Invalid or expired email verification token.");
            }

            user.EmailConfirmed = true;
            user.EmailConfirmedAtUtc = DateTime.UtcNow;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiresAtUtc = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();

            await _notificationClient.SendAccountCreatedAsync(
                new AccountCreatedNotificationRequest(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName));
        }

        public async Task ResendEmailVerificationAsync(ResendEmailVerificationRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || user.EmailConfirmed)
                return;

            var verificationToken = GenerateToken();
            var verificationTokenExpiresAtUtc = DateTime.UtcNow.AddHours(EmailVerificationTokenLifetimeHours);

            user.EmailVerificationTokenHash = HashToken(verificationToken);
            user.EmailVerificationTokenExpiresAtUtc = verificationTokenExpiresAtUtc;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();

            await _notificationClient.SendEmailVerificationAsync(
                new EmailVerificationNotificationRequest(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    verificationToken,
                    verificationTokenExpiresAtUtc,
                    BuildActionUrl("AuthLinks:EmailVerificationUrl", user.Email, verificationToken)));
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !user.EmailConfirmed || !user.IsActive)
                return;

            var resetToken = GenerateToken();
            var resetTokenExpiresAtUtc = DateTime.UtcNow.AddHours(PasswordResetTokenLifetimeHours);

            user.PasswordResetTokenHash = HashToken(resetToken);
            user.PasswordResetTokenExpiresAtUtc = resetTokenExpiresAtUtc;
            user.PasswordResetRequestedAtUtc = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();

            await _notificationClient.SendPasswordResetAsync(
                new PasswordResetNotificationRequest(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    resetToken,
                    resetTokenExpiresAtUtc,
                    BuildActionUrl("AuthLinks:PasswordResetUrl", user.Email, resetToken)));
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                throw new Exception("Passwords do not match.");

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null ||
                !user.EmailConfirmed ||
                !user.IsActive ||
                string.IsNullOrWhiteSpace(user.PasswordResetTokenHash) ||
                user.PasswordResetTokenExpiresAtUtc == null ||
                user.PasswordResetTokenExpiresAtUtc < DateTime.UtcNow ||
                !TokenMatches(request.Token, user.PasswordResetTokenHash))
            {
                throw new Exception("Invalid or expired password reset token.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiresAtUtc = null;
            user.PasswordResetRequestedAtUtc = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
                throw new Exception("Invalid email or password.");

            var passwordIsValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!passwordIsValid)
                throw new Exception("Invalid email or password.");

            if (!user.EmailConfirmed)
                throw new Exception("Email address has not been verified.");

            if (!user.IsActive)
                throw new Exception("User account is inactive.");

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = roles
                }
            };
        }

        private string? BuildActionUrl(string configurationKey, string email, string token)
        {
            var actionUrl = _configuration[configurationKey];
            if (string.IsNullOrWhiteSpace(actionUrl))
                return null;

            var encodedEmail = WebUtility.UrlEncode(email);
            var encodedToken = WebUtility.UrlEncode(token);

            if (actionUrl.Contains("{email}", StringComparison.OrdinalIgnoreCase) ||
                actionUrl.Contains("{token}", StringComparison.OrdinalIgnoreCase))
            {
                return actionUrl
                    .Replace("{email}", encodedEmail, StringComparison.OrdinalIgnoreCase)
                    .Replace("{token}", encodedToken, StringComparison.OrdinalIgnoreCase);
            }

            var separator = actionUrl.Contains('?') ? "&" : "?";
            return $"{actionUrl}{separator}email={encodedEmail}&token={encodedToken}";
        }

        private static UserDto MapUser(User user, IReadOnlyCollection<string> roles)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = roles.ToList()
            };
        }

        private static string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string HashToken(string token)
        {
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var hashBytes = SHA256.HashData(tokenBytes);
            return Convert.ToHexString(hashBytes);
        }

        private static bool TokenMatches(string token, string expectedHash)
        {
            var actualHash = HashToken(token);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(actualHash),
                Encoding.UTF8.GetBytes(expectedHash));
        }
    }
}
