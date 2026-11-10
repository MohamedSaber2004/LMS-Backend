namespace LMS.Application.Features.Packages.DTOs
{
    public class LearnerPackageDto
    {
        public Guid Id { get; set; }
        public Guid InstructorId { get; set; }
        public string InstructorFullName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public decimal RemainingHours { get; set; }
        public int RemainingFullSessions => (int)Math.Floor(RemainingHours / 1.0m);
        public int RemainingHalfSessions => (int)Math.Floor(RemainingHours / 0.5m);
        public decimal HourlyRateSnapshot { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

