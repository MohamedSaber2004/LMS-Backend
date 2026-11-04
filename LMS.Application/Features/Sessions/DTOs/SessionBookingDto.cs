using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class SessionBookingDto
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public Guid LearnerId { get; set; }

        public string LearnerFullName { get; set; } = string.Empty;

        public BookingStatus Status { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancelReason { get; set; }
    }
}
