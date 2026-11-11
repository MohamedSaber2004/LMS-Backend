using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class SessionDto
    {
        public Guid Id { get; set; }

        public Guid InstructorId { get; set; }

        public string InstructorFullName { get; set; } = string.Empty;

        public string? Title { get; set; }

        public SessionType Type { get; set; }

        public DateTime StartsAtUtc { get; set; }

        public DateTime EndsAtUtc { get; set; }

        public SessionStatus Status { get; set; }

        public decimal Price { get; set; }

        public int DurationMinutes { get; set; }

        public bool IsTrial { get; set; }

        public bool IsPaid { get; set; }

        public DateTime? PaidAtUtc { get; set; }

        public bool IsAttendanceConfirmed { get; set; }

        public int SeatCapacity { get; set; }

        public int SeatsBooked { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancelReason { get; set; }

        public Guid? MyBookingId { get; set; }

        public BookingStatus? MyBookingStatus { get; set; }

        public LiveSessionInfoDto? Live { get; set; }
    }

    public class LiveSessionInfoDto
    {
        public Guid RoomId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool CanJoinNow { get; set; }
        public DateTime JoinOpensAtUtc { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? EndedAtUtc { get; set; }
    }
}
