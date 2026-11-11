using System;
using LMS.Application.Common.Options;
using LMS.Application.Features.Sessions.DTOs;
using LMS.Domain.Entities;
using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions
{
    public static class SessionMapper
    {
        public static SessionDto ToDto(
            Session session,
            string instructorFullName,
            SessionBooking? myBooking = null,
            int joinLeadMinutes = 15,
            int joinTailMinutes = SchedulingOptions.DefaultJoinTailMinutes) => new()
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
            DurationMinutes = session.DurationMinutes,
            IsTrial = session.IsTrial,
            IsPaid = session.IsPaid,
            PaidAtUtc = session.PaidAtUtc,
            IsAttendanceConfirmed = session.IsAttendanceConfirmed,
            SeatCapacity = session.SeatCapacity,
            SeatsBooked = session.SeatsBooked,
            CancelledAt = session.CancelledAt,
            CancelReason = session.CancelReason,
            MyBookingId = myBooking?.Id,
            MyBookingStatus = myBooking?.Status,
            Live = session.LiveRoom == null ? null : new LiveSessionInfoDto
            {
                RoomId = session.LiveRoom.Id,
                Status = session.LiveRoom.Status.ToString(),
                JoinOpensAtUtc = session.StartsAtUtc.AddMinutes(-joinLeadMinutes),
                CanJoinNow = session.Status == SessionStatus.Confirmed &&
                             session.LiveRoom.Status != LiveRoomStatus.Ended &&
                             DateTime.UtcNow >= session.StartsAtUtc.AddMinutes(-joinLeadMinutes) &&
                             DateTime.UtcNow <= session.EndsAtUtc.AddMinutes(joinTailMinutes),
                StartedAtUtc = session.LiveRoom.StartedAtUtc,
                EndedAtUtc = session.LiveRoom.EndedAtUtc
            }
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
