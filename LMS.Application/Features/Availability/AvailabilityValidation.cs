using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Availability
{
    internal static class AvailabilityValidation
    {
        public static async Task<bool> HasNoOverlapAsync(
            ILmsDbContext context,
            Guid instructorId,
            DayOfWeek day,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid? excludedSlotId,
            CancellationToken cancellationToken)
        {
            if (instructorId == Guid.Empty || endTime <= startTime)
            {
                return true;
            }

            return !await context.InstructorAvailabilitySlots.AnyAsync(
                slot => slot.InstructorId == instructorId
                    && slot.Day == day
                    && slot.Id != excludedSlotId
                    && slot.StartTime < endTime
                    && slot.EndTime > startTime,
                cancellationToken);
        }

        public static async Task<bool> InstructorExistsAsync(
            ILmsDbContext context,
            Guid instructorId,
            CancellationToken cancellationToken)
        {
            return instructorId != Guid.Empty
                && await context.Instructors.AnyAsync(instructor => instructor.Id == instructorId, cancellationToken);
        }

        public static async Task<bool> InstructorIsApprovedAsync(
            ILmsDbContext context,
            Guid instructorId,
            CancellationToken cancellationToken)
        {
            return instructorId != Guid.Empty
                && await context.Instructors.AnyAsync(
                    instructor => instructor.Id == instructorId
                        && instructor.Application != null
                        && instructor.Application.Status == InstructorApplicationStatus.Approved
                        && instructor.User.IsActive,
                    cancellationToken);
        }

        public static async Task<TimeZoneInfo?> ResolveInstructorZoneAsync(
            ILmsDbContext context,
            Guid instructorId,
            CancellationToken cancellationToken)
        {
            var timezoneId = await CountryTimezoneResolver.ForInstructorAsync(
                context, instructorId, cancellationToken);

            return TimezoneHelper.TryResolve(timezoneId, out var zone) ? zone : null;
        }

        public static async Task<bool> HasNoLiveSessionsAsync(
            ILmsDbContext context,
            Guid slotId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return !await context.Sessions.AnyAsync(
                session => (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
                    && session.EndsAtUtc > now
                    && context.AvailabilityInstances.Any(instance => instance.WeeklySlotId == slotId
                        && instance.InstructorId == session.InstructorId
                        && instance.StartsAtUtc < session.EndsAtUtc
                        && instance.EndsAtUtc > session.StartsAtUtc),
                cancellationToken);
        }

        /// <summary>
        /// A dated day cannot be closed while the instructor still has to teach on it: the sessions on it
        /// have to be rescheduled or called off first.
        /// </summary>
        public static async Task<bool> HasNoLiveSessionsOnDayAsync(
            ILmsDbContext context,
            Guid instructorId,
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return !await context.Sessions.AnyAsync(
                session => session.InstructorId == instructorId
                    && (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
                    && session.EndsAtUtc > now
                    && context.AvailabilityInstances.Any(instance => instance.InstructorId == instructorId
                        && instance.Date == date
                        && instance.StartsAtUtc < session.EndsAtUtc
                        && instance.EndsAtUtc > session.StartsAtUtc),
                cancellationToken);
        }
    }
}
