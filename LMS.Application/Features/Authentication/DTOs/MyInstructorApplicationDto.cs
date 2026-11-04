using LMS.Domain.Enums;

namespace LMS.Application.Features.Authentication.DTOs
{
    public class MyInstructorApplicationDto
    {
        public MyInstructorApplicationDto() { }

        public MyInstructorApplicationDto(
            Guid applicationId,
            InstructorApplicationStatus status,
            decimal hourlyRate,
            bool isActive,
            bool emailConfirmed,
            DateTime submittedAt,
            DateTime? reviewedAt,
            string? reviewNotes,
            int certificatesCount,
            bool hasIntroVideo)
        {
            ApplicationId = applicationId;
            Status = status;
            HourlyRate = hourlyRate;
            IsActive = isActive;
            EmailConfirmed = emailConfirmed;
            SubmittedAt = submittedAt;
            ReviewedAt = reviewedAt;
            ReviewNotes = reviewNotes;
            CertificatesCount = certificatesCount;
            HasIntroVideo = hasIntroVideo;
        }

        public Guid ApplicationId { get; set; }
        public InstructorApplicationStatus Status { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public int CertificatesCount { get; set; }
        public bool HasIntroVideo { get; set; }
    }
}
