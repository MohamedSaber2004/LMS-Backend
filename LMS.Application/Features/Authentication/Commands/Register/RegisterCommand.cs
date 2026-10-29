using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LMS.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommand : IRequest<Result<RegisterResponseDto>>
    {
        public RegisterCommand() { }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public Guid CountryId { get; set; }
        public UserType Role { get; set; } = UserType.Learner;

        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? LinkedInUrl { get; set; }
        public List<IFormFile>? Certificates { get; set; }
    }
}
