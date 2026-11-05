using LMS.Application.Features.Authentication.DTOs;
using LMS.Domain.Enums;

namespace LMS.Application.Features.InstructorProfile.DTOs
{
    public class InstructorProfileDto
    {
        public InstructorProfileDto() { }

        public InstructorProfileDto(
            Guid instructorId,
            Guid userId,
            string specialization,
            string bio,
            int yearsOfExperience,
            string education,
            string? linkedInUrl,
            decimal hourlyRate,
            InstructorApplicationStatus applicationStatus,
            bool isActive,
            IReadOnlyCollection<InstructorCertificateDto> certificates,
            DateTime createdAt,
            DateTime? updatedAt)
        {
            InstructorId = instructorId;
            UserId = userId;
            Specialization = specialization;
            Bio = bio;
            YearsOfExperience = yearsOfExperience;
            Education = education;
            LinkedInUrl = linkedInUrl;
            HourlyRate = hourlyRate;
            ApplicationStatus = applicationStatus;
            IsActive = isActive;
            Certificates = certificates;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public Guid InstructorId { get; set; }
        public Guid UserId { get; set; }
        public UserProfileDto Profile { get; set; } = new();
        public bool VacationMode { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Education { get; set; } = string.Empty;
        public string? LinkedInUrl { get; set; }
        public decimal HourlyRate { get; set; }
        public InstructorApplicationStatus ApplicationStatus { get; set; }
        public bool IsActive { get; set; }
        public IReadOnlyCollection<InstructorCertificateDto> Certificates { get; set; }
            = Array.Empty<InstructorCertificateDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
