using LMS.Application.Common.Interfaces;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Sessions
{
    internal static class SessionsValidation
    {
        /// <summary>
        /// Everything that can stop a learner from taking a dated availability window, checked in the
        /// order a learner would want to be told about it.
        /// </summary>
        public static async Task<BookingBlocker> FindBookingBlockerAsync(
            ILmsDbContext context,
            Guid instructorId,
            Guid learnerId,
            Guid availabilityInstanceId,
            int breakMinutes,
            CancellationToken cancellationToken)
        {
            var isBookable = await InstructorIsBookableAsync(context, instructorId, cancellationToken);

            if (!isBookable)
            {
                return BookingBlocker.InstructorNotBookable;
            }

            var instance = await context.AvailabilityInstances.AsNoTracking()
                .Where(row => row.Id == availabilityInstanceId && row.InstructorId == instructorId)
                .Select(row => new { row.StartsAtUtc, row.EndsAtUtc, row.IsOpen })
                .FirstOrDefaultAsync(cancellationToken);

            if (instance is null || !instance.IsOpen)
            {
                return BookingBlocker.InstanceMissing;
            }

            if (instance.EndsAtUtc <= DateTime.UtcNow)
            {
                return BookingBlocker.SlotElapsed;
            }

            var startsAtUtc = instance.StartsAtUtc;
            var endsAtUtc = instance.EndsAtUtc;
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

        /// <summary>
        /// Only an approved, active instructor who is not on vacation can receive a new booking.
        /// Confirmed sessions of a vacationing instructor are untouched.
        /// </summary>
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

        /// <summary>
        /// A session the instructor still has to teach overlaps the window. Pending counts too: the time
        /// is held from the moment the learner books, not from the moment the instructor confirms.
        /// </summary>
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

        /// <summary>Same check from the learner's side, across every instructor they booked with.</summary>
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

        /// <summary>
        /// A session both sides can still act on: not finished, not already called off.
        /// </summary>
        public static bool IsLiveSession(Session? session) => session is not null
            && (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
            && session.EndsAtUtc > DateTime.UtcNow;

        /// <summary>
        /// A proposed window must still sit inside one of the instructor's open dated rows, otherwise the
        /// session drifts off the schedule the instructor published.
        /// </summary>
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

        /// <summary>
        /// Re-checked when a reschedule is approved, not when it was asked for: the calendars of both
        /// sides may have filled up in the meantime.
        /// </summary>
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

        /// <summary>
        /// The lock is measured against the start, so time still left before it must cover the actor's
        /// whole window. Once it does not, that side has lost the right to move or cancel the session.
        /// </summary>
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
