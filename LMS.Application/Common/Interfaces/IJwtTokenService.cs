using LMS.Domain.Entities;

namespace LMS.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessToken(
            ApplicationUser user,
            IList<string> roles,
            CancellationToken cancellationToken = default);

        string GenerateRefreshToken(ApplicationUser user);

        Guid? ValidateRefreshToken(string token);
    }
}
