using LMS.Application.Features.Sessions.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Features.Sessions
{
    public static class SessionMapper
    {
        public static SessionDto ToDto(
            Session session,
            string instructorFullName,
            SessionBooking? myBooking = null) => new()
        {
            Id = session.Id,
            InstructorId = session.InstructorId,
            InstructorFullName = instructorFullName,
            Title = session.Title,
            Type = session.Type,
            StartsAtUtc = session.StartsAtUtc,
            EndsAtUtc = session.EndsAtUtc,
            Status = session.Status,
            Price = session.Price,
            SeatCapacity = session.SeatCapacity,
            SeatsBooked = session.SeatsBooked,
            CancelledAt = session.CancelledAt,
            CancelReason = session.CancelReason,
            MyBookingId = myBooking?.Id,
            MyBookingStatus = myBooking?.Status
        };

        public static SessionBookingDto ToBookingDto(SessionBooking booking, string learnerFullName) => new()
        {
            Id = booking.Id,
            SessionId = booking.SessionId,
            LearnerId = booking.LearnerId,
            LearnerFullName = learnerFullName,
            Status = booking.Status,
            CancelledAt = booking.CancelledAt,
            CancelReason = booking.CancelReason
        };

        public static RescheduleRequestDto ToRescheduleDto(RescheduleRequest request) => new()
        {
            Id = request.Id,
            SessionId = request.SessionId,
            RequestedByUserId = request.RequestedByUserId,
            RequestedByType = request.RequestedByType,
            FromUtc = request.FromUtc,
            ToUtc = request.ToUtc,
            Status = request.Status,
            CreatedAt = request.CreatedAt
        };
    }
}
