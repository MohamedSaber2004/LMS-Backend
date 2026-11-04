using LMS.Application.Common.Helpers;
using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LMS.Infrastructure.Services.Scheduling
{
    public class AvailabilityMaterializationService : IAvailabilityMaterializationService
    {
        private readonly ILmsDbContext _context;
        private readonly ILogger<AvailabilityMaterializationService> _logger;
        private readonly SchedulingOptions _options;

        public AvailabilityMaterializationService(
            ILmsDbContext context,
            IOptions<SchedulingOptions> options,
            ILogger<AvailabilityMaterializationService> logger)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<int> MaterializeAllAsync(CancellationToken cancellationToken = default)
        {
            var batchSize = Math.Max(1, _options.InstructorBatchSize);
            var written = 0;
            var skipped = 0;

            for (var skip = 0; ; skip += batchSize)
            {
                var instructorIds = await _context.Instructors.AsNoTracking()
                    .Where(instructor => instructor.Application != null
                        && instructor.Application.Status == InstructorApplicationStatus.Approved
                        && instructor.User.IsActive
                        && instructor.AvailabilitySlots.Count > 0)
                    .OrderBy(instructor => instructor.Id)
                    .Skip(skip)
                    .Take(batchSize)
                    .Select(instructor => instructor.Id)
                    .ToListAsync(cancellationToken);

                if (instructorIds.Count == 0)
                {
                    break;
                }

                foreach (var instructorId in instructorIds)
                {
                    var changed = await MaterializeInstructorAsync(instructorId, cancellationToken);
                    if (changed < 0)
                    {
                        skipped++;
                        continue;
                    }

                    written += changed;
                }

                if (instructorIds.Count < batchSize)
                {
                    break;
                }
            }

            if (skipped > 0)
            {
                _logger.LogWarning(
                    "Availability materialization wrote {Written} rows and skipped {Skipped} instructors with no resolvable time zone.",
                    written,
                    skipped);
            }

            return written;
        }

        public async Task<int> MaterializeInstructorAsync(Guid instructorId, CancellationToken cancellationToken = default)
        {
            var timezoneId = await CountryTimezoneResolver.ForInstructorAsync(_context, instructorId, cancellationToken);

            TimeZoneInfo? zone;
            if (string.IsNullOrWhiteSpace(timezoneId)
                || !TimezoneHelper.TryResolve(timezoneId, out zone)
                || zone is null)
            {
                return -1;
            }

            var daysAhead = Math.Clamp(_options.DaysAhead, 1, Math.Max(1, _options.MaxDaysAhead));
            var firstDate = TimezoneHelper.LocalToday(zone);
            var lastDate = firstDate.AddDays(daysAhead - 1);

            var slots = await _context.InstructorAvailabilitySlots.AsNoTracking()
                .Where(slot => slot.InstructorId == instructorId)
                .ToListAsync(cancellationToken);

            var existing = await _context.AvailabilityInstances
                .Where(instance => instance.InstructorId == instructorId
                    && instance.Date >= firstDate
                    && instance.Date <= lastDate)
                .ToListAsync(cancellationToken);

            var protectedDates = await GetProtectedDatesAsync(instructorId, firstDate, lastDate, zone, cancellationToken);

            var current = existing
                .Where(instance => instance.WeeklySlotId != null)
                .GroupBy(instance => (instance.Date, instance.WeeklySlotId!.Value))
                .ToDictionary(group => group.Key, group => group.First());

            var desired = BuildDesiredOccurrences(slots, firstDate, lastDate, zone);

            var changed = 0;

            foreach (var occurrence in desired)
            {
                if (protectedDates.Contains(occurrence.Key.Date) || current.ContainsKey(occurrence.Key))
                {
                    continue;
                }

                _context.AvailabilityInstances.Add(AvailabilityInstance.Create(
                    instructorId,
                    occurrence.Key.WeeklySlotId,
                    occurrence.Key.Date,
                    occurrence.Value.StartsAtUtc,
                    occurrence.Value.EndsAtUtc));
                changed++;
            }

            foreach (var instance in current.Values)
            {
                if (!instance.IsOpen
                    || instance.WeeklySlotId is null
                    || protectedDates.Contains(instance.Date))
                {
                    continue;
                }

                var key = (instance.Date, instance.WeeklySlotId.Value);
                if (!desired.TryGetValue(key, out var occurrence))
                {
                    _context.AvailabilityInstances.Remove(instance);
                    changed++;
                    continue;
                }

                if (instance.StartsAtUtc != occurrence.StartsAtUtc || instance.EndsAtUtc != occurrence.EndsAtUtc)
                {
                    instance.MoveTo(occurrence.StartsAtUtc, occurrence.EndsAtUtc);
                    changed++;
                }
            }

            if (changed > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return changed;
        }

        private async Task<HashSet<DateOnly>> GetProtectedDatesAsync(
            Guid instructorId,
            DateOnly firstDate,
            DateOnly lastDate,
            TimeZoneInfo zone,
            CancellationToken cancellationToken)
        {
            if (!TimezoneHelper.TryToUtcDayBounds(firstDate, zone, out var horizonStartUtc, out _)
                || !TimezoneHelper.TryToUtcDayBounds(lastDate, zone, out _, out var horizonEndUtc))
            {
                return new HashSet<DateOnly>();
            }

            var now = DateTime.UtcNow;

            var liveSessions = await _context.Sessions.AsNoTracking()
                .Where(session => session.InstructorId == instructorId
                    && (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
                    && session.EndsAtUtc > now
                    && session.StartsAtUtc < horizonEndUtc
                    && session.StartsAtUtc >= horizonStartUtc)
                .Select(session => session.StartsAtUtc)
                .ToListAsync(cancellationToken);

            var dates = new HashSet<DateOnly>();
            foreach (var startsAtUtc in liveSessions)
            {
                TimezoneHelper.ToWallClock(startsAtUtc, zone, out var localDate, out _);
                dates.Add(localDate);
            }

            return dates;
        }

        private static Dictionary<(DateOnly Date, Guid WeeklySlotId), (DateTime StartsAtUtc, DateTime EndsAtUtc)>
            BuildDesiredOccurrences(
                IReadOnlyCollection<InstructorAvailabilitySlot> slots,
                DateOnly firstDate,
                DateOnly lastDate,
                TimeZoneInfo zone)
        {
            var occurrences = new Dictionary<(DateOnly, Guid), (DateTime, DateTime)>();

            for (var date = firstDate; date <= lastDate; date = date.AddDays(1))
            {
                var weekday = date.DayOfWeek;

                foreach (var slot in slots.Where(slot => slot.Day == weekday))
                {
                    if (TimezoneHelper.TryToUtc(date, slot.StartTime, zone, out var startsAtUtc)
                        && TimezoneHelper.TryToUtc(date, slot.EndTime, zone, out var endsAtUtc))
                    {
                        occurrences[(date, slot.Id)] = (startsAtUtc, endsAtUtc);
                    }
                }
            }

            return occurrences;
        }
    }
}
