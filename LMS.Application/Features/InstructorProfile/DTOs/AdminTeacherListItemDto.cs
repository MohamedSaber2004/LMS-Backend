namespace LMS.Application.Features.InstructorProfile.DTOs
{
    public class AdminTeacherListItemDto
    {
        public Guid InstructorId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Country { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public decimal CommissionRate { get; set; }
        public bool IsNativeSpeaker { get; set; }
        public IReadOnlyCollection<string> Languages { get; set; } = Array.Empty<string>();
        public decimal RatingAverage { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedSessionsCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime JoinedAtUtc { get; set; }
    }
}

