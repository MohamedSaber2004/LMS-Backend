using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class SessionParticipantDto
    {
        public Guid BookingId { get; set; }
        public Guid LearnerId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public DateTime BookedAt { get; set; }
        public AttendanceStatus? AttendanceStatus { get; set; }
        public DateTime? JoinedAtUtc { get; set; }
        public DateTime? LeftAtUtc { get; set; }
        public int? DurationMinutes { get; set; }
        public string? AttendanceNotes { get; set; }
    }
}
