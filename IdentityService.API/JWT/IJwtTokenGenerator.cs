using IdentityService.API.Models;

namespace IdentityService.API.JWT;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IList<string> roles);
}
