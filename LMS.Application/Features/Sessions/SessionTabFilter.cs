using LMS.Domain.Entities;
using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions
{
    internal static class SessionTabFilter
    {
        public static IQueryable<Session> Apply(IQueryable<Session> query, SessionTab tab)
        {
            var now = DateTime.UtcNow;

            return tab switch
            {
                SessionTab.Upcoming => query.Where(session =>
                    (session.Status == SessionStatus.Pending || session.Status == SessionStatus.Confirmed)
                    && session.EndsAtUtc > now),
                SessionTab.Completed => query.Where(session => session.Status == SessionStatus.Completed),
                _ => query.Where(session =>
                    session.Status == SessionStatus.Cancelled || session.Status == SessionStatus.NoShow)
            };
        }

        public static IQueryable<SessionBooking> ApplyToBookings(IQueryable<SessionBooking> query, SessionTab tab)
        {
            var now = DateTime.UtcNow;

            return tab switch
            {
                SessionTab.Upcoming => query.Where(booking =>
                    (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
                    && (booking.Session.Status == SessionStatus.Pending || booking.Session.Status == SessionStatus.Confirmed)
                    && booking.Session.EndsAtUtc > now),
                SessionTab.Completed => query.Where(booking => booking.Session.Status == SessionStatus.Completed),
                _ => query.Where(booking =>
                    !(booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed)
                    || booking.Session.Status == SessionStatus.Cancelled
                    || booking.Session.Status == SessionStatus.NoShow)
            };
        }
    }
}
