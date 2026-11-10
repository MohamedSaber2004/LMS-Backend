namespace LMS.Application.Features.InstructorProfile.DTOs
{
    public class TeacherLearnerItemDto
    {
        public Guid LearnerId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Country { get; set; }
        public decimal TotalHoursPurchased { get; set; }
        public decimal RemainingHours { get; set; }
        public int TotalSessionsCount { get; set; }
        public int CompletedSessionsCount { get; set; }
        public DateTime? LastSessionDateUtc { get; set; }
    }
}

