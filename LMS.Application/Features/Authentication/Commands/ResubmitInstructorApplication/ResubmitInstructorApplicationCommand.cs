using LMS.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Authentication.Commands.ResubmitInstructorApplication
{
    public class ResubmitInstructorApplicationCommand : IRequest<Result<string>>
    {
        public ResubmitInstructorApplicationCommand() { }

        public string Specialization { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public int? YearsOfExperience { get; set; }

        public string? Education { get; set; }

        public string? LinkedInUrl { get; set; }

        public decimal? HourlyRate { get; set; }

        public List<IFormFile> Certificates { get; set; } = new();
    }
}
