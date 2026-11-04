using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Domain.Entities;

namespace LMS.Application.Features.InstructorProfile
{
    public static class InstructorProfileMapper
    {
        public static InstructorProfileDto ToDto(Instructor instructor)
        {
            var certificates = instructor.Certificates
                .Select(certificate => new InstructorCertificateDto(
                    certificate.Id,
                    certificate.FilePath,
                    certificate.OriginalFileName,
                    certificate.Type))
                .ToArray();

            return new InstructorProfileDto(
                instructor.Id,
                instructor.UserId,
                instructor.Specialization,
                instructor.Bio,
                instructor.YearsOfExperience,
                instructor.Education,
                instructor.LinkedInUrl,
                instructor.HourlyRate,
                instructor.Application!.Status,
                instructor.User.IsActive,
                certificates,
                instructor.CreatedAt,
                instructor.UpdatedAt);
        }
    }
}
