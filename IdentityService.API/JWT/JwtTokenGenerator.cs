using IdentityService.API.Models;

namespace IdentityService.API.JWT;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    string IJwtTokenGenerator.GenerateToken(User user, IList<string> roles)
    {
        throw new NotImplementedException();
    }
}
