using LMS.Application.Common.Interfaces;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services.Scheduling
{
    public class SessionLifecycleService : ISessionLifecycleService
    {
        private const int BatchSize = 500;

        private readonly ILmsDbContext _context;

        public SessionLifecycleService(ILmsDbContext context)
        {
            _context = context;
        }

        public async Task<int> ExpireElapsedSessionsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var elapsed = await _context.Sessions
                .Where(session => (session.Status == SessionStatus.Pending
                                || session.Status == SessionStatus.Confirmed)
                    && session.EndsAtUtc <= now)
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
    }
}
