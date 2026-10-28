using LMS.Application.Common.Models;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Domain.Enums;
using MediatR;

namespace LMS.Application.Features.Authentication.Commands.UpdateUserInformation
{
    public class UpdateUserInformationCommand : IRequest<Result<UserProfileDto>>
    {
        public UpdateUserInformationCommand() { }

        public UpdateUserInformationCommand(
            string fullName,
            Guid? countryId = null,
            Language? language = null,
            string? profilePictureUrl = null)
        {
            FullName = fullName;
            CountryId = countryId;
            Language = language;
            ProfilePictureUrl = profilePictureUrl;
        }

        public string FullName { get; set; } = string.Empty;
        public Guid? CountryId { get; set; }
        public Language? Language { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
