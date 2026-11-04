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

        public int SeatCapacity { get; set; }

        public int SeatsBooked { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancelReason { get; set; }

        /// <summary>Populated only for a learner caller, so their own booking state travels with the session.</summary>
        public Guid? MyBookingId { get; set; }

        public BookingStatus? MyBookingStatus { get; set; }
    }
}
