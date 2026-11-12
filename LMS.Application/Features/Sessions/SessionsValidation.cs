using LMS.Application.Common.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Sessions
{
    internal static class SessionsValidation
    {
        public static int ResolveDurationMinutes(AcademicSessionDuration duration)
            => (int)duration is 25 or 50 ? (int)duration : 50;

        public static async Task<bool> HasFreeTrialAsync(
            ILmsDbContext context,
            Guid learnerId,
            CancellationToken cancellationToken)
        {
            return !await context.Sessions.AnyAsync(
                session => session.Bookings.Any(booking => booking.LearnerId == learnerId)
                    && session.IsTrial
                    && session.Status != SessionStatus.Cancelled,
                cancellationToken);
        }

        public static async Task<BookingSlotPick> PickBookableSlotOnDateAsync(
            ILmsDbContext context,
            Guid instructorId,
            Guid learnerId,
            DateOnly date,
            int durationMinutes,
            int breakMinutes,
            CancellationToken cancellationToken)
        {
            if (!await InstructorIsBookableAsync(context, instructorId, cancellationToken))
            {
                return new BookingSlotPick
                {
                    Status = BookingSlotStatus.InstructorNotBookable,
                    Blocker = BookingBlocker.InstructorNotBookable
                };
            }

            var now = DateTime.UtcNow;

            var openWindows = await context.AvailabilityInstances.AsNoTracking()
                .Where(instance => instance.InstructorId == instructorId
                    && instance.Date == date
                    && instance.IsOpen)
                .OrderBy(instance => instance.StartsAtUtc)
                .Select(instance => new { instance.StartsAtUtc, instance.EndsAtUtc })
                .ToListAsync(cancellationToken);

            if (openWindows.Count == 0)
            {
                return new BookingSlotPick { Status = BookingSlotStatus.NoWindowsOnDate };
            }

            var upcomingWindows = openWindows
                .Where(window => window.StartsAtUtc > now)
                .ToList();

            if (upcomingWindows.Count == 0)
            {
                return new BookingSlotPick
                {
                    Status = BookingSlotStatus.AllWindowsElapsed,
                    WindowCount = openWindows.Count
                };
            }

            var fittingWindows = upcomingWindows
                .Where(window => (window.EndsAtUtc - window.StartsAtUtc).TotalMinutes >= durationMinutes)
                .ToList();

            if (fittingWindows.Count == 0)
            {
                return new BookingSlotPick
                {
                    Status = BookingSlotStatus.NoWindowFitsDuration,
                    WindowCount = upcomingWindows.Count
                };
            }

            BookingBlocker? firstBlocker = null;

            foreach (var window in fittingWindows)
            {
                var blocker = await FindWindowBlockerAsync(
                    context, instructorId, learnerId, window.StartsAtUtc, window.EndsAtUtc, breakMinutes, cancellationToken);

                if (blocker == BookingBlocker.None)
                {
                    return new BookingSlotPick
                    {
                        Status = BookingSlotStatus.Bookable,
                        StartsAtUtc = window.StartsAtUtc,
                        EndsAtUtc = window.EndsAtUtc,
                        WindowCount = fittingWindows.Count
                    };
                }

                firstBlocker ??= blocker;
            }

            return new BookingSlotPick
            {
                Status = BookingSlotStatus.AllWindowsBlocked,
                Blocker = firstBlocker ?? BookingBlocker.None,
                WindowCount = fittingWindows.Count
            };
        }

        private static async Task<BookingBlocker> FindWindowBlockerAsync(
            ILmsDbContext context,
            Guid instructorId,
            Guid learnerId,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            int breakMinutes,
            CancellationToken cancellationToken)
        {
            var padded = TimeSpan.FromMinutes(breakMinutes);

            if (await HasInstructorOverlapAsync(context, instructorId, startsAtUtc, endsAtUtc, null, cancellationToken))
            {
                return BookingBlocker.InstructorConflict;
            }

            if (await HasInstructorOverlapAsync(
                    context, instructorId, startsAtUtc - padded, endsAtUtc + padded, null, cancellationToken))
            {
                return BookingBlocker.InstructorBreak;
            }

            if (await HasLearnerOverlapAsync(context, learnerId, startsAtUtc, endsAtUtc, null, cancellationToken))
            {
                return BookingBlocker.LearnerConflict;
            }

            if (await HasLearnerOverlapAsync(
                    context, learnerId, startsAtUtc - padded, endsAtUtc + padded, null, cancellationToken))
            {
                return BookingBlocker.LearnerBreak;
            }

            return BookingBlocker.None;
        }

        public static async Task<bool> InstructorIsBookableAsync(
            ILmsDbContext context,
            Guid instructorId,
            CancellationToken cancellationToken)
        {
            return instructorId != Guid.Empty
                && await context.Instructors.AnyAsync(
                    instructor => instructor.Id == instructorId
                        && !instructor.VacationMode
                        && instructor.User.IsActive
                        && instructor.Application != null
                        && instructor.Application.Status == InstructorApplicationStatus.Approved,
                    cancellationToken);
        }

        public static Task<bool> HasInstructorOverlapAsync(
            ILmsDbContext context,
            Guid instructorId,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            Guid? excludedSessionId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return context.Sessions.AnyAsync(
                session => session.InstructorId == instructorId
                    && (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
                    && session.EndsAtUtc > now
                    && session.Id != excludedSessionId
                    && session.StartsAtUtc < endsAtUtc
                    && session.EndsAtUtc > startsAtUtc,
                cancellationToken);
        }

        public static Task<bool> HasLearnerOverlapAsync(
            ILmsDbContext context,
            Guid learnerId,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            Guid? excludedSessionId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return context.SessionBookings.AnyAsync(
                booking => booking.LearnerId == learnerId
                    && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
                    && (booking.Session.Status == SessionStatus.Pending || booking.Session.Status == SessionStatus.Confirmed)
                    && booking.Session.EndsAtUtc > now
                    && booking.SessionId != excludedSessionId
                    && booking.Session.StartsAtUtc < endsAtUtc
                    && booking.Session.EndsAtUtc > startsAtUtc,
                cancellationToken);
        }

        public static bool IsLiveSession(Session? session) => session is not null
            && (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
            && session.EndsAtUtc > DateTime.UtcNow;

        public static Task<bool> WindowIsInsideAvailabilityAsync(
            ILmsDbContext context,
            Guid instructorId,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            CancellationToken cancellationToken)
        {
            return context.AvailabilityInstances.AnyAsync(
                instance => instance.InstructorId == instructorId
                    && instance.IsOpen
                    && instance.StartsAtUtc <= startsAtUtc
                    && instance.EndsAtUtc >= endsAtUtc,
                cancellationToken);
        }

        public static async Task<bool> NewWindowIsFreeAsync(
            ILmsDbContext context,
            Guid sessionId,
            Guid instructorId,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            int breakMinutes,
            CancellationToken cancellationToken)
        {
            var padded = TimeSpan.FromMinutes(breakMinutes);

            if (await HasInstructorOverlapAsync(context, instructorId, startsAtUtc, endsAtUtc, sessionId, cancellationToken)
                || await HasInstructorOverlapAsync(
                    context, instructorId, startsAtUtc - padded, endsAtUtc + padded, sessionId, cancellationToken))
            {
                return false;
            }

            var learnerIds = await context.SessionBookings
                .Where(booking => booking.SessionId == sessionId
                    && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed))
                .Select(booking => booking.LearnerId)
                .ToListAsync(cancellationToken);

            foreach (var learnerId in learnerIds)
            {
                if (await HasLearnerOverlapAsync(context, learnerId, startsAtUtc, endsAtUtc, sessionId, cancellationToken)
                    || await HasLearnerOverlapAsync(
                        context, learnerId, startsAtUtc - padded, endsAtUtc + padded, sessionId, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsOutsideLock(DateTime startsAtUtc, int lockHours)
            => startsAtUtc - DateTime.UtcNow >= TimeSpan.FromHours(lockHours);

        public static async Task<bool> HasNoPendingRescheduleAsync(
            ILmsDbContext context,
            Guid sessionId,
            CancellationToken cancellationToken)
        {
            return !await context.RescheduleRequests.AnyAsync(
                request => request.SessionId == sessionId
                    && request.Status == RescheduleRequestStatus.Pending,
                cancellationToken);
        }

        public static Task<bool> InstructorOwnsSessionAsync(
            ILmsDbContext context,
            Guid sessionId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            return context.Sessions.AnyAsync(
                session => session.Id == sessionId
                    && session.Instructor != null
                    && session.Instructor.UserId == userId,
                cancellationToken);
        }

        public static Task<bool> LearnerHoldsBookingOnSessionAsync(
            ILmsDbContext context,
            Guid sessionId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            return context.SessionBookings.AnyAsync(
                booking => booking.SessionId == sessionId
                    && booking.Learner != null
                    && booking.Learner.UserId == userId
                    && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed),
                cancellationToken);
        }
    }
}
