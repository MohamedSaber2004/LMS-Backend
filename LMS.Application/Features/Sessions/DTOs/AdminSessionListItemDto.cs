using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class AdminSessionListItemDto
    {
        public Guid SessionId { get; set; }
        public Guid InstructorId { get; set; }
        public string InstructorFullName { get; set; } = string.Empty;
        public string InstructorEmail { get; set; } = string.Empty;
        public Guid? LearnerId { get; set; }
        public string LearnerFullName { get; set; } = string.Empty;
        public string LearnerEmail { get; set; } = string.Empty;
        public DateTime StartsAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsTrial { get; set; }
        public SessionStatus Status { get; set; }
        public bool IsAttendanceConfirmed { get; set; }
        public decimal HourlyRateSnapshot { get; set; }
        public decimal CommissionRateSnapshot { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}

