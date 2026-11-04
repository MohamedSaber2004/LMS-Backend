using LMS.Application.Common.Helpers;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Persistence.Seeding
{
    public static class InstructorAvailabilitySeeder
    {
        private static readonly (DayOfWeek Day, TimeSpan StartTime, TimeSpan EndTime)[] WeeklyPattern =
        {
            (DayOfWeek.Monday, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)),
            (DayOfWeek.Monday, new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)),
            (DayOfWeek.Tuesday, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)),
            (DayOfWeek.Tuesday, new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)),
            (DayOfWeek.Wednesday, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)),
            (DayOfWeek.Wednesday, new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)),
            (DayOfWeek.Thursday, new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0)),
            (DayOfWeek.Thursday, new TimeSpan(14, 0, 0), new TimeSpan(16, 0, 0)),
            (DayOfWeek.Saturday, new TimeSpan(11, 0, 0), new TimeSpan(12, 30, 0))
        };

        public static async Task<int> SeedAsync(
            LmsDbContext context,
            int minimumBreakMinutes,
            CancellationToken cancellationToken = default)
        {
            var instructorIds = await context.Instructors
                .Where(instructor => instructor.Application != null
                    && instructor.Application.Status == InstructorApplicationStatus.Approved
                    && instructor.User.IsActive)
                .OrderBy(instructor => instructor.Id)
                .Select(instructor => instructor.Id)
                .ToListAsync(cancellationToken);

            var added = 0;

            foreach (var instructorId in instructorIds)
            {
                var timezoneId = await CountryTimezoneResolver.ForInstructorAsync(
                    context, instructorId, cancellationToken);

                if (!TimezoneHelper.TryResolve(timezoneId, out var zone) || zone is null)
                {
                    continue;
                }

                added += await SeedInstructorAsync(context, instructorId, minimumBreakMinutes, cancellationToken);
            }

            return added;
        }

        private static async Task<int> SeedInstructorAsync(
            LmsDbContext context,
            Guid instructorId,
            int minimumBreakMinutes,
            CancellationToken cancellationToken)
        {
            var taken = (await context.InstructorAvailabilitySlots
                .Where(slot => slot.InstructorId == instructorId)
                .Select(slot => new { slot.Day, slot.StartTime, slot.EndTime })
                .ToListAsync(cancellationToken))
                .Select(window => new Window(window.Day, window.StartTime, window.EndTime))
                .ToList();

            var abandoned = (await context.InstructorAvailabilitySlots
                .IgnoreQueryFilters()
                .Where(slot => slot.InstructorId == instructorId && slot.IsDeleted)
                .Select(slot => new { slot.Day, slot.StartTime, slot.EndTime })
                .ToListAsync(cancellationToken))
                .Select(window => new Window(window.Day, window.StartTime, window.EndTime))
                .ToList();

            var pad = TimeSpan.FromMinutes(Math.Max(0, minimumBreakMinutes));
            var created = new List<InstructorAvailabilitySlot>();

            foreach (var candidate in WeeklyPattern)
            {
                if (abandoned.Any(window => window.SameAs(candidate.Day, candidate.StartTime, candidate.EndTime)))
                {
                    continue;
                }

                var collides = taken.Any(window => window.Day == candidate.Day
                    && window.EndTime > candidate.StartTime - pad
                    && window.StartTime < candidate.EndTime + pad);

                if (collides)
                {
                    continue;
                }

                created.Add(InstructorAvailabilitySlot.Create(
                    instructorId, candidate.Day, candidate.StartTime, candidate.EndTime));

                taken.Add(new Window(candidate.Day, candidate.StartTime, candidate.EndTime));
            }

            if (created.Count == 0)
            {
                return 0;
            }

            await context.InstructorAvailabilitySlots.AddRangeAsync(created, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return created.Count;
        }

        private sealed record Window(DayOfWeek Day, TimeSpan StartTime, TimeSpan EndTime)
        {
            public bool SameAs(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
                => Day == day && StartTime == startTime && EndTime == endTime;
        }
    }
}
