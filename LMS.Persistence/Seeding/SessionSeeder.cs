using LMS.Application.Common.Helpers;
using LMS.Application.Common.Options;
using LMS.Domain.Entities;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Persistence.Seeding
{
    /// <summary>
    /// Fills the scheduling surfaces with demo data built only from rows that already exist: every session is a
    /// real dated window of a real approved instructor, taken whole exactly the way the booking endpoint takes
    /// it, and every learner is someone already registered. Nothing here invents a person or a time.
    /// </summary>
    public static class SessionSeeder
    {
        private const int SessionsPerInstructor = 4;

        /// <summary>One session per this many free windows, so the learner-facing list never runs dry.</summary>
        private const int FreeWindowsPerSession = 3;

        private const int HistoryPerInstructor = 2;
        private const int HistoryDaysBack = 21;

        private const string LearnerCancelReason = "Learner could not keep this week.";

        public static async Task<int> SeedAsync(
            LmsDbContext context,
            SchedulingOptions scheduling,
            CancellationToken cancellationToken = default)
        {
            var pad = TimeSpan.FromMinutes(Math.Max(0, scheduling.MinimumBreakMinutes));

            var teachers = (await context.Instructors
                .Where(instructor => instructor.Application != null
                    && instructor.Application.Status == InstructorApplicationStatus.Approved
                    && instructor.User.IsActive)
                .OrderBy(instructor => instructor.Id)
                .Select(instructor => new
                {
                    instructor.Id,
                    UserId = instructor.User.Id,
                    instructor.HourlyRate
                })
                .ToListAsync(cancellationToken))
                .Select(row => new Teacher(row.Id, row.UserId, row.HourlyRate))
                .ToList();

            var learners = (await context.Learners
                .Where(learner => learner.User.IsActive)
                .OrderBy(learner => learner.UserId)
                .Select(learner => new { learner.Id, learner.UserId })
                .ToListAsync(cancellationToken))
                .Select(row => new Learner(row.Id, row.UserId))
                .ToList();

            if (teachers.Count == 0 || learners.Count == 0)
            {
                return 0;
            }

            // Only a holding booking keeps time: a cancelled or declined one gave its seat and its slice of the
            // calendar back, so it must not stop a new session from being placed on the same window.
            var learnerBusy = (await context.SessionBookings
                .Select(booking => new
                {
                    booking.LearnerId,
                    booking.Session.StartsAtUtc,
                    booking.Session.EndsAtUtc,
                    booking.Session.Status,
                    BookingStatus = booking.Status
                })
                .ToListAsync(cancellationToken))
                .Where(row => row.BookingStatus == BookingStatus.Pending || row.BookingStatus == BookingStatus.Confirmed)
                .GroupBy(row => row.LearnerId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(row => new Span(row.StartsAtUtc, row.EndsAtUtc, IsLive(row.Status)))
                        .ToList());

            var written = 0;

            foreach (var teacher in teachers)
            {
                written += await SeedUpcomingAsync(
                    context, teacher, learners, learnerBusy, pad, cancellationToken);

                written += await SeedHistoryAsync(
                    context, teacher, learners, learnerBusy, pad, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);

            foreach (var teacher in teachers)
            {
                written += await SeedRescheduleAsync(context, teacher, scheduling, pad, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);

            return written;
        }

        /// <summary>
        /// Books windows no session has ever touched, so a second run of the seeder adds nothing.
        /// </summary>
        private static async Task<int> SeedUpcomingAsync(
            LmsDbContext context,
            Teacher teacher,
            IReadOnlyList<Learner> learners,
            Dictionary<Guid, List<Span>> learnerBusy,
            TimeSpan pad,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var instances = (await context.AvailabilityInstances
                .Where(instance => instance.InstructorId == teacher.InstructorId
                    && instance.IsOpen
                    && instance.EndsAtUtc > now)
                .OrderBy(instance => instance.StartsAtUtc)
                .Select(instance => new { instance.StartsAtUtc, instance.EndsAtUtc })
                .ToListAsync(cancellationToken))
                .Select(row => new Span(row.StartsAtUtc, row.EndsAtUtc, false))
                .ToList();

            var taken = (await context.Sessions
                .Where(session => session.InstructorId == teacher.InstructorId)
                .Select(session => new { session.StartsAtUtc, session.EndsAtUtc, session.Status })
                .ToListAsync(cancellationToken))
                .Select(row => new Span(row.StartsAtUtc, row.EndsAtUtc, IsLive(row.Status)))
                .ToList();

            var free = instances
                .Where(instance => !taken.Any(session => session.Overlaps(instance.StartsAtUtc, instance.EndsAtUtc)))
                .Where(instance => !taken.Any(session => session.BreaksRule(
                    instance.StartsAtUtc, instance.EndsAtUtc, pad)))
                .ToList();

            // The cap counts what the instructor already has, real bookings included, so a second run of the
            // seeder tops the demo set up to the same number instead of stacking another set on top of it.
            var room = SessionsPerInstructor - taken.Count;
            var written = 0;

            for (var index = 0; index < free.Count && written < room; index += FreeWindowsPerSession)
            {
                var instance = free[index];
                var learner = NextFreeLearner(
                    learners, learnerBusy, instance.StartsAtUtc, instance.EndsAtUtc, pad, written);

                if (learner is null)
                {
                    continue;
                }

                var session = Session.Create(
                    teacher.InstructorId,
                    SessionType.OneToOne,
                    instance.StartsAtUtc,
                    instance.EndsAtUtc,
                    teacher.HourlyRate);

                var booking = SessionBooking.Create(session.Id, learner.LearnerId);
                session.ReserveSeat();

                // The states the endpoints produce, in a fixed rotation, so every tab of the session list has
                // real rows: confirmed, waiting on the teacher, called off by the learner, declined by the teacher.
                switch (written % 4)
                {
                    case 0:
                        booking.Confirm();
                        session.Confirm();
                        break;
                    case 2:
                        booking.Cancel(learner.UserId, LearnerCancelReason);
                        session.Cancel(learner.UserId, LearnerCancelReason);
                        session.ReleaseSeat();
                        break;
                    case 3:
                        booking.Decline(teacher.UserId);
                        session.Cancel(teacher.UserId, null);
                        session.ReleaseSeat();
                        break;
                }

                context.Sessions.Add(session);
                context.SessionBookings.Add(booking);

                taken.Add(new Span(instance.StartsAtUtc, instance.EndsAtUtc, IsLive(session.Status)));

                if (booking.HoldsSeat())
                {
                    Hold(learnerBusy, learner.LearnerId, instance.StartsAtUtc, instance.EndsAtUtc);
                }

                written++;
            }

            return written;
        }

        /// <summary>
        /// Back-dates the last windows of the published pattern and runs them to the end, because the materializer
        /// only writes today forward and a Completed row needs a past to sit in.
        /// </summary>
        private static async Task<int> SeedHistoryAsync(
            LmsDbContext context,
            Teacher teacher,
            IReadOnlyList<Learner> learners,
            Dictionary<Guid, List<Span>> learnerBusy,
            TimeSpan pad,
            CancellationToken cancellationToken)
        {
            var timezoneId = await CountryTimezoneResolver.ForInstructorAsync(
                context, teacher.InstructorId, cancellationToken);

            if (!TimezoneHelper.TryResolve(timezoneId, out var zone) || zone is null)
            {
                return 0;
            }

            var slots = (await context.InstructorAvailabilitySlots
                .Where(slot => slot.InstructorId == teacher.InstructorId)
                .Select(slot => new { slot.Id, slot.Day, slot.StartTime, slot.EndTime })
                .ToListAsync(cancellationToken))
                .Select(row => new WeeklyRow(row.Id, row.Day, row.StartTime, row.EndTime))
                .ToList();

            if (slots.Count == 0)
            {
                return 0;
            }

            var instanceKeys = (await context.AvailabilityInstances
                .IgnoreQueryFilters()
                .Where(instance => instance.InstructorId == teacher.InstructorId && instance.WeeklySlotId != null)
                .Select(instance => new { instance.Date, instance.WeeklySlotId })
                .ToListAsync(cancellationToken))
                .Select(row => new SlotDate(row.Date, row.WeeklySlotId!.Value))
                .ToList();

            var sessionBounds = (await context.Sessions
                .IgnoreQueryFilters()
                .Where(session => session.InstructorId == teacher.InstructorId)
                .Select(session => new { session.StartsAtUtc, session.EndsAtUtc })
                .ToListAsync(cancellationToken))
                .Select(row => new Span(row.StartsAtUtc, row.EndsAtUtc, false))
                .ToList();

            var today = TimezoneHelper.LocalToday(zone);

            // Past rows the instructor already has count against the same cap, so history is a one-shot fill
            // instead of something that grows by two sessions every time the seeder runs.
            var now = DateTime.UtcNow;
            var room = HistoryPerInstructor - sessionBounds.Count(span => span.EndsAtUtc <= now);

            if (room <= 0)
            {
                return 0;
            }

            var written = 0;

            for (var offset = 1; offset <= HistoryDaysBack && written < room; offset++)
            {
                var date = today.AddDays(-offset);

                foreach (var slot in slots.Where(row => row.Day == date.DayOfWeek).OrderBy(row => row.StartTime))
                {
                    if (!TimezoneHelper.TryToUtc(date, slot.StartTime, zone, out var startsAtUtc)
                        || !TimezoneHelper.TryToUtc(date, slot.EndTime, zone, out var endsAtUtc))
                    {
                        continue;
                    }

                    var key = new SlotDate(date, slot.Id);

                    if (instanceKeys.Any(existing => existing.SameAs(key))
                        || sessionBounds.Any(session => session.Overlaps(startsAtUtc, endsAtUtc)))
                    {
                        continue;
                    }

                    var learner = NextFreeLearner(
                        learners, learnerBusy, startsAtUtc, endsAtUtc, pad, written);

                    if (learner is null)
                    {
                        continue;
                    }

                    context.AvailabilityInstances.Add(AvailabilityInstance.Create(
                        teacher.InstructorId, slot.Id, date, startsAtUtc, endsAtUtc));

                    var session = Session.Create(
                        teacher.InstructorId, SessionType.OneToOne, startsAtUtc, endsAtUtc, teacher.HourlyRate);

                    var booking = SessionBooking.Create(session.Id, learner.LearnerId);
                    session.ReserveSeat();
                    booking.Confirm();
                    session.Confirm();

                    // The lifecycle job would complete this once the window elapsed; the row is written already
                    // elapsed, so the state it lands in is the state the job produces.
                    session.Complete();

                    context.Sessions.Add(session);
                    context.SessionBookings.Add(booking);

                    instanceKeys.Add(key);
                    sessionBounds.Add(new Span(startsAtUtc, endsAtUtc, false));
                    Hold(learnerBusy, learner.LearnerId, startsAtUtc, endsAtUtc);

                    written++;
                    break;
                }
            }

            return written;
        }

        /// <summary>
        /// One ask the approve endpoint would still accept: the lesson keeps its length, the target window is
        /// still open and free for both sides, and the session is far enough ahead to be movable at all.
        /// </summary>
        private static async Task<int> SeedRescheduleAsync(
            LmsDbContext context,
            Teacher teacher,
            SchedulingOptions scheduling,
            TimeSpan pad,
            CancellationToken cancellationToken)
        {
            var earliestStart = DateTime.UtcNow.AddHours(Math.Max(1, scheduling.InstructorLockHours));

            // One ask per instructor, so running the seeder twice does not pile a second request onto the
            // next session it happens to look at.
            var instructorHasPendingAsk = await context.RescheduleRequests.AnyAsync(
                request => request.Session != null
                    && request.Session.InstructorId == teacher.InstructorId
                    && request.Status == RescheduleRequestStatus.Pending,
                cancellationToken);

            if (instructorHasPendingAsk)
            {
                return 0;
            }

            var session = await context.Sessions
                .Where(candidate => candidate.InstructorId == teacher.InstructorId
                    && (candidate.Status == SessionStatus.Pending || candidate.Status == SessionStatus.Confirmed)
                    && candidate.StartsAtUtc > earliestStart)
                .OrderBy(candidate => candidate.StartsAtUtc)
                .Select(candidate => new { candidate.Id, candidate.StartsAtUtc, candidate.EndsAtUtc })
                .FirstOrDefaultAsync(cancellationToken);

            if (session is null)
            {
                return 0;
            }

            var duration = session.EndsAtUtc - session.StartsAtUtc;

            var holders = (await context.SessionBookings
                .Where(booking => booking.SessionId == session.Id
                    && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed))
                .Select(booking => new { booking.LearnerId, booking.Learner.UserId })
                .ToListAsync(cancellationToken))
                .Select(row => new Learner(row.LearnerId, row.UserId))
                .ToList();

            if (holders.Count == 0)
            {
                return 0;
            }

            var otherSessions = (await context.Sessions
                .Where(other => other.InstructorId == teacher.InstructorId
                    && other.Id != session.Id
                    && (other.Status == SessionStatus.Pending || other.Status == SessionStatus.Confirmed))
                .Select(other => new { other.StartsAtUtc, other.EndsAtUtc })
                .ToListAsync(cancellationToken))
                .Select(row => new Span(row.StartsAtUtc, row.EndsAtUtc, true))
                .ToList();

            var busyByLearner = new Dictionary<Guid, List<Span>>();

            foreach (var holder in holders)
            {
                busyByLearner[holder.LearnerId] = (await context.SessionBookings
                    .Where(booking => booking.LearnerId == holder.LearnerId
                        && booking.SessionId != session.Id
                        && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed))
                    .Select(booking => new { booking.Session.StartsAtUtc, booking.Session.EndsAtUtc })
                    .ToListAsync(cancellationToken))
                    .Select(row => new Span(row.StartsAtUtc, row.EndsAtUtc, true))
                    .ToList();
            }

            var windows = await context.AvailabilityInstances
                .Where(instance => instance.InstructorId == teacher.InstructorId
                    && instance.IsOpen
                    && instance.StartsAtUtc > earliestStart
                    && instance.StartsAtUtc != session.StartsAtUtc)
                .OrderBy(instance => instance.StartsAtUtc)
                .Select(instance => new { instance.StartsAtUtc, instance.EndsAtUtc })
                .ToListAsync(cancellationToken);

            // SQL Server cannot subtract datetime2 rows, so the length is matched after the read.
            var targets = windows
                .Where(candidate => candidate.EndsAtUtc - candidate.StartsAtUtc == duration)
                .ToList();

            var target = targets.FirstOrDefault(candidate =>
                !otherSessions.Any(other => other.BreaksRule(
                    candidate.StartsAtUtc, candidate.EndsAtUtc, pad))
                && !holders.Any(holder => busyByLearner[holder.LearnerId].Any(busy => busy.BreaksRule(
                    candidate.StartsAtUtc, candidate.EndsAtUtc, pad))));

            if (target is null)
            {
                return 0;
            }

            context.RescheduleRequests.Add(RescheduleRequest.Create(
                session.Id,
                holders[0].UserId,
                UserType.Learner,
                target.StartsAtUtc,
                target.EndsAtUtc));

            return 1;
        }

        private static Learner? NextFreeLearner(
            IReadOnlyList<Learner> learners,
            Dictionary<Guid, List<Span>> learnerBusy,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            TimeSpan pad,
            int cursor)
        {
            for (var offset = 0; offset < learners.Count; offset++)
            {
                var learner = learners[(cursor + offset) % learners.Count];

                var busy = learnerBusy.TryGetValue(learner.LearnerId, out var spans)
                    ? spans
                    : new List<Span>();

                if (!busy.Any(span => span.BreaksRule(startsAtUtc, endsAtUtc, pad)))
                {
                    return learner;
                }
            }

            return null;
        }

        private static void Hold(
            Dictionary<Guid, List<Span>> learnerBusy,
            Guid learnerId,
            DateTime startsAtUtc,
            DateTime endsAtUtc)
        {
            if (!learnerBusy.TryGetValue(learnerId, out var spans))
            {
                spans = new List<Span>();
                learnerBusy[learnerId] = spans;
            }

            spans.Add(new Span(startsAtUtc, endsAtUtc, true));
        }

        private static bool IsLive(SessionStatus status)
            => status == SessionStatus.Pending || status == SessionStatus.Confirmed;

        private sealed record Teacher(Guid InstructorId, Guid UserId, decimal HourlyRate);

        private sealed record Learner(Guid LearnerId, Guid UserId);

        private sealed record WeeklyRow(Guid Id, DayOfWeek Day, TimeSpan StartTime, TimeSpan EndTime);

        private sealed record SlotDate(DateOnly Date, Guid WeeklySlotId)
        {
            public bool SameAs(SlotDate other) => Date == other.Date && WeeklySlotId == other.WeeklySlotId;
        }

        private sealed record Span(DateTime StartsAtUtc, DateTime EndsAtUtc, bool IsLive)
        {
            public bool Overlaps(DateTime startsAtUtc, DateTime endsAtUtc)
                => StartsAtUtc < endsAtUtc && EndsAtUtc > startsAtUtc;

            public bool BreaksRule(DateTime startsAtUtc, DateTime endsAtUtc, TimeSpan pad)
                => IsLive && StartsAtUtc < endsAtUtc + pad && EndsAtUtc > startsAtUtc - pad;
        }
    }
}
