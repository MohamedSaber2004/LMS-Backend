using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Enums;
using LMS.API.Services;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Common.Helpers
{
    public static class ActorOwnership
    {
        public static bool IsAdmin(ICurrentUserService currentUser)
            => currentUser.UserTypes.HasValue
               && (currentUser.UserTypes.Value & (int)UserType.Admin) != 0;

        public static bool HasAdminBoardAccess(ICurrentUserService currentUser, AdminPermission permission)
            => IsAdmin(currentUser) || currentUser.PermissionCodes.Contains((int)permission);

        public static async Task<Guid?> ResolveInstructorIdAsync(
            ILmsDbContext context,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }

            return await context.Instructors.AsNoTracking()
                .Where(instructor => instructor.UserId == userId)
                .Select(instructor => (Guid?)instructor.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<Guid?> ResolveLearnerIdAsync(
            ILmsDbContext context,
            Guid userId,
            CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
            {
                return null;
            }

            return await context.Learners.AsNoTracking()
                .Where(learner => learner.UserId == userId)
                .Select(learner => (Guid?)learner.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<Guid> ResolveWriteTargetAsync(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            Guid requestedInstructorId,
            AdminPermission adminBoardPermission,
            CancellationToken cancellationToken)
        {
            if (HasAdminBoardAccess(currentUser, adminBoardPermission))
            {
                if (requestedInstructorId == Guid.Empty)
                {
                    throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
                }

                return requestedInstructorId;
            }

            var ownInstructorId = await ResolveInstructorIdAsync(context, currentUser.UserId, cancellationToken);

            if (ownInstructorId is null
                || (requestedInstructorId != Guid.Empty && requestedInstructorId != ownInstructorId.Value))
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            return ownInstructorId.Value;
        }

        public static async Task EnsureCanWriteForAsync(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            Guid ownerInstructorId,
            AdminPermission adminBoardPermission,
            CancellationToken cancellationToken)
        {
            if (HasAdminBoardAccess(currentUser, adminBoardPermission))
            {
                return;
            }

            var ownInstructorId = await ResolveInstructorIdAsync(context, currentUser.UserId, cancellationToken);

            if (ownInstructorId != ownerInstructorId)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }
        }
    }
}
