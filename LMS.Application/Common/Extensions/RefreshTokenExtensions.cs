using LMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Common.Extensions
{
    public static class RefreshTokenExtensions
    {
        public static async Task RevokeAllRefreshTokensAsync(
            this ILmsDbContext context,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var activeTokens = await context.UserRefreshTokens
                .Where(token => token.UserId == userId && !token.IsRevoked)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Revoke();
            }
        }
    }
}
