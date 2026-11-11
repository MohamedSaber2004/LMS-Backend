using LMS.API.Services;
using LMS.Application.Common.Interfaces;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Common.Helpers
{
    public static class SuperAdminAccountGuard
    {
        public static async Task<bool> TargetsSuperAdminAccountAsync(
            ILmsDbContext context,
            Guid userId,
            CancellationToken cancellationToken)
            => await context.UserRoles
                .AnyAsync(userRole => userRole.UserId == userId
                    && context.Roles.Any(role => role.Id == userRole.RoleId
                        && role.Name == nameof(UserType.Admin)),
                    cancellationToken);
    }
}
