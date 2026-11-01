namespace LMS.Application.Features.Onboarding.DTOs
{
    public class InstructorCardDto
    {
        public InstructorCardDto() { }

        public InstructorCardDto(
            Guid id,
            Guid userId,
            string fullName,
            string specialization,
            string bio,
            int yearsOfExperience,
            string education,
            string? linkedInUrl,
            decimal hourlyRate,
            string? countryName,
            string? introVideoPath,
            IReadOnlyCollection<string> certificatePaths,
            DateTime approvedAt)
        {
            Id = id;
            UserId = userId;
            FullName = fullName;
            Specialization = specialization;
            Bio = bio;
            YearsOfExperience = yearsOfExperience;
            Education = education;
            LinkedInUrl = linkedInUrl;
            HourlyRate = hourlyRate;
            CountryName = countryName;
            IntroVideoPath = introVideoPath;
            CertificatePaths = certificatePaths;
            ApprovedAt = approvedAt;
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Education { get; set; } = string.Empty;
        public string? LinkedInUrl { get; set; }
        public decimal HourlyRate { get; set; }
        public string? CountryName { get; set; }
        public string? IntroVideoPath { get; set; }
        public IReadOnlyCollection<string> CertificatePaths { get; set; } = Array.Empty<string>();
        public DateTime ApprovedAt { get; set; }
    }
}
