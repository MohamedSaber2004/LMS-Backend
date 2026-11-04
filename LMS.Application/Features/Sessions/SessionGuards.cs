using LMS.API.Services;
using LMS.Application.Common.Exceptions;
using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Localization;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Sessions
{
    public enum SessionActor
    {
        Teacher = 0,
        Learner = 1
    }

    internal static class SessionGuards
    {
        public static async Task<SessionActor> ResolveActorAsync(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            Session session,
            CancellationToken cancellationToken)
        {
            var instructorId = await ActorOwnership.ResolveInstructorIdAsync(
                context, currentUser.UserId, cancellationToken);

            if (instructorId == session.InstructorId)
            {
                return SessionActor.Teacher;
            }

            var learnerId = await ActorOwnership.ResolveLearnerIdAsync(
                context, currentUser.UserId, cancellationToken);

            if (learnerId is null)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            var id = learnerId.Value;

            var holdsSeat = await context.SessionBookings.AnyAsync(
                booking => booking.SessionId == session.Id
                    && booking.LearnerId == id
                    && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
                cancellationToken);

            if (!holdsSeat)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }

            return SessionActor.Learner;
        }

        public static async Task EnsureTeacherAsync(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            Session session,
            CancellationToken cancellationToken)
        {
            var actor = await ResolveActorAsync(context, currentUser, session, cancellationToken);

            if (actor != SessionActor.Teacher)
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }
        }

        public static async Task EnsureOtherSideAsync(
            ILmsDbContext context,
            ICurrentUserService currentUser,
            Session session,
            UserType requestedByType,
            CancellationToken cancellationToken)
        {
            var actor = await ResolveActorAsync(context, currentUser, session, cancellationToken);

            if ((actor == SessionActor.Teacher) == (requestedByType == UserType.Instructor))
            {
                throw new ForbiddenException(LocalizationKeys.ExceptionMessages.Forbidden);
            }
        }
    }
}
