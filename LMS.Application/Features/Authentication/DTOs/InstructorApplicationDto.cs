using LMS.Domain.Enums;

namespace LMS.Application.Features.Authentication.DTOs
{
    public class InstructorApplicationDto
    {
        public InstructorApplicationDto() { }

        public InstructorApplicationDto(
            Guid id,
            Guid userId,
            string fullName,
            string email,
            string phoneNumber,
            Guid? countryId,
            string? countryName,
            string specialization,
            string bio,
            int yearsOfExperience,
            string? linkedInUrl,
            InstructorApplicationStatus status,
            DateTime submittedAt,
            Guid? reviewedByUserId,
            DateTime? reviewedAt,
            string? reviewNotes,
            IReadOnlyCollection<InstructorCertificateDto> certificates)
        {
            Id = id;
            UserId = userId;
            FullName = fullName;
            Email = email;
            PhoneNumber = phoneNumber;
            CountryId = countryId;
            CountryName = countryName;
            Specialization = specialization;
            Bio = bio;
            YearsOfExperience = yearsOfExperience;
            LinkedInUrl = linkedInUrl;
            Status = status;
            SubmittedAt = submittedAt;
            ReviewedByUserId = reviewedByUserId;
            ReviewedAt = reviewedAt;
            ReviewNotes = reviewNotes;
            Certificates = certificates;
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid? CountryId { get; set; }
        public string? CountryName { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public InstructorApplicationStatus Status { get; set; }
        public DateTime SubmittedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
        public IReadOnlyCollection<InstructorCertificateDto> Certificates { get; set; } = Array.Empty<InstructorCertificateDto>();
    }
}
