using LMS.Application.Common.Models;
using LMS.Application.Features.InstructorProfile.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.InstructorProfile.Commands.UpdateInstructorProfile
{
    public class UpdateInstructorProfileCommand : IRequest<Result<InstructorProfileDto>>
    {
        public UpdateInstructorProfileCommand() { }

        public string? FullName { get; set; }

        public Guid? CountryId { get; set; }

        public Language? Language { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string Specialization { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public int? YearsOfExperience { get; set; }

        public string? Education { get; set; }

        public string? LinkedInUrl { get; set; }

        public decimal? HourlyRate { get; set; }
    }
}
