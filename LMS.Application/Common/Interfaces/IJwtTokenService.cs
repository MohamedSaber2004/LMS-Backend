using LMS.Domain.Entities;

namespace LMS.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);

        string GenerateRefreshToken(ApplicationUser user);
    }
}
