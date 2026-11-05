using LMS.Application.Common.Interfaces;
using LMS.Application.Features.Authentication.DTOs;
using LMS.Application.Common.Helpers;
using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Features.Authentication
{
    public static class UserProfileMapper
    {
        public static async Task<UserProfileDto> ToDtoAsync(
            ILmsDbContext context,
            ApplicationUser user,
            IList<string> roles,
            CancellationToken cancellationToken)
        {
            var countryName = user.CountryId.HasValue
                ? await context.Countries
                    .Where(country => country.Id == user.CountryId.Value)
                    .Select(country => country.CountryEnName)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

            var timezoneId = await CountryTimezoneResolver.ResolveAsync(
                context, user.CountryId, user.PhoneNumber, cancellationToken);

            var instructorId = await context.Instructors
                .Where(instructor => instructor.UserId == user.Id)
                .Select(instructor => (Guid?)instructor.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return new UserProfileDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                user.ProfilePictureUrl,
                user.CountryId,
                countryName,
                user.Language,
                roles,
                user.CreatedAt)
            {
                TimezoneId = timezoneId,
                InstructorId = instructorId
            };
        }
    }
}
