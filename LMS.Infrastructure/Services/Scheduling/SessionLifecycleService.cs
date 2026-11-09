using LMS.Application.Common.Interfaces;
using LMS.Application.Common.Options;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LMS.Infrastructure.Services.Scheduling
{
    public class SessionLifecycleService : ISessionLifecycleService
    {
        private const int BatchSize = 500;

        private readonly ILmsDbContext _context;
        private readonly SchedulingOptions _options;

        public SessionLifecycleService(ILmsDbContext context, IOptions<SchedulingOptions> options)
        {
            _context = context;
            _options = options.Value;
        }

        public async Task<int> ExpireElapsedSessionsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var confirmedCutoff = now.AddMinutes(-_options.JoinTailMinutes);

            var elapsed = await _context.Sessions
                .Where(session => (session.Status == SessionStatus.Pending && session.EndsAtUtc <= now)
                                || (session.Status == SessionStatus.Confirmed && session.EndsAtUtc <= confirmedCutoff))
                .OrderBy(session => session.EndsAtUtc)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (elapsed.Count == 0)
            {
                return 0;
            }

            var unconfirmedIds = elapsed
                .Where(session => session.Status == SessionStatus.Pending)
                .Select(session => session.Id)
                .ToList();

            var heldSeats = unconfirmedIds.Count == 0
                ? new List<Domain.Entities.SessionBooking>()
                : await _context.SessionBookings
                    .Where(booking => unconfirmedIds.Contains(booking.SessionId)
                        && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed))
                    .ToListAsync(cancellationToken);

            var sessionsById = elapsed.ToDictionary(session => session.Id);

            foreach (var booking in heldSeats)
            {
                booking.Cancel(null, null);
                if (sessionsById.TryGetValue(booking.SessionId, out var session))
                {
                    session.ReleaseSeat();
                }
            }

            foreach (var session in elapsed)
            {
                if (session.Status == SessionStatus.Confirmed)
                {
                    session.Complete();
                }
                else
                {
                    session.Cancel(null, null);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return elapsed.Count;
        }

        public async Task<int> CleanupOrphanedLiveRoomsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var cutoff = now.AddMinutes(-_options.JoinTailMinutes);

            var orphanedRooms = await _context.LiveRooms
                .Include(r => r.Session)
                .Include(r => r.Participants)
                .Where(r => (r.Status == LiveRoomStatus.Live || r.Status == LiveRoomStatus.NotStarted)
                         && r.Session.EndsAtUtc <= cutoff)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (orphanedRooms.Count == 0)
            {
                return 0;
            }

            foreach (var room in orphanedRooms)
            {
                if (room.Status == LiveRoomStatus.Live)
                {
                    room.End(now);

                    var openParticipants = room.Participants
                        .Where(p => p.LeftAtUtc == null)
                        .ToList();

                    foreach (var participant in openParticipants)
                    {
                        participant.Leave(now);
                    }
                }
                else if (room.Status == LiveRoomStatus.NotStarted)
                {
                    room.MarkAbandoned(now);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return orphanedRooms.Count;
        }
    }
}
