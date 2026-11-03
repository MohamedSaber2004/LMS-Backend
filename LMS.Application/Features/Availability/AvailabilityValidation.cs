using LMS.Application.Common.Interfaces;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Availability
{
    internal static class AvailabilityValidation
    {
        /// <summary>
        /// True when no other active window on the same day intersects [startTime, endTime).
        /// Soft-deleted slots are already excluded by the query filter.
        /// </summary>
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
    }
}
