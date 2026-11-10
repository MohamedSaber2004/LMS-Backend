namespace LMS.Application.Features.Packages.DTOs
{
    public class AdminPackageItemDto
    {
        public Guid PackageId { get; set; }
        public Guid LearnerId { get; set; }
        public string LearnerFullName { get; set; } = string.Empty;
        public string LearnerEmail { get; set; } = string.Empty;
        public Guid InstructorId { get; set; }
        public string InstructorFullName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public decimal RemainingHours { get; set; }
        public decimal HourlyRateSnapshot { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}

