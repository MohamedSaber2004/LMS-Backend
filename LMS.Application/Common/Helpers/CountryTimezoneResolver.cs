using LMS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Common.Helpers
{
    public static class CountryTimezoneResolver
    {
        public static async Task<string?> ForInstructorAsync(
            ILmsDbContext context,
            Guid instructorId,
            CancellationToken cancellationToken)
        {
            if (instructorId == Guid.Empty)
            {
                return null;
            }

            var anchor = await context.Instructors.AsNoTracking()
                .Where(instructor => instructor.Id == instructorId)
                .Select(instructor => new { instructor.User.CountryId, instructor.User.PhoneNumber })
                .FirstOrDefaultAsync(cancellationToken);

            return anchor is null
                ? null
                : await ResolveAsync(context, anchor.CountryId, anchor.PhoneNumber, cancellationToken);
        }

        public static async Task<string?> ResolveAsync(
            ILmsDbContext context,
            Guid? countryId,
            string? phone,
            CancellationToken cancellationToken)
        {
            if (countryId.HasValue && countryId.Value != Guid.Empty)
            {
                var fromProfile = await context.Countries.AsNoTracking()
                    .Where(country => country.Id == countryId.Value)
                    .Select(country => country.TimezoneId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (TimezoneHelper.IsValidIanaId(fromProfile))
                {
                    return fromProfile!.Trim();
                }
            }

            return await ForPhoneAsync(context, phone, cancellationToken);
        }

        public static async Task<string?> ForPhoneAsync(
            ILmsDbContext context,
            string? phone,
            CancellationToken cancellationToken)
        {
            var normalized = PhoneNumberValidationHelper.Normalize(phone);
            if (normalized.Length == 0 || !normalized.StartsWith('+'))
            {
                return null;
            }

            var candidates = await context.Countries.AsNoTracking()
                .Where(country => country.TimezoneId != null && country.TimezoneId != string.Empty)
                .Select(country => new { country.Code, country.TimezoneId })
                .ToListAsync(cancellationToken);

            var match = candidates
                .Where(candidate => IsDialPrefix(candidate.Code, normalized))
                .OrderByDescending(candidate => candidate.Code!.Length)
                .FirstOrDefault();

            return match is null || !TimezoneHelper.IsValidIanaId(match.TimezoneId)
                ? null
                : match.TimezoneId!.Trim();
        }

        private static bool IsDialPrefix(string? code, string normalizedPhone)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            var dialCode = code.Trim();
            if (!dialCode.StartsWith('+'))
            {
                dialCode = "+" + dialCode.TrimStart('+', '0');
            }

            return dialCode.Length > 1 && normalizedPhone.StartsWith(dialCode, StringComparison.Ordinal);
        }
    }
}
