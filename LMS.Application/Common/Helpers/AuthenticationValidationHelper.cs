using System.Security.Cryptography;
using System.Text;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LMS.Application.Common.Helpers
{
    public static class AuthenticationValidationHelper
    {
        public static bool IsOtpExpired(DateTime? expiry)
            => !expiry.HasValue || expiry.Value <= DateTime.UtcNow;

        public static bool IsOtpValid(string? storedCode, DateTime? expiry, string submittedCode)
            => !IsOtpExpired(expiry) && CodesMatch(storedCode, submittedCode);

        public static async Task<bool> MeetsPasswordPolicyAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string password)
        {
            foreach (var validator in userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(userManager, user, password);
                if (!result.Succeeded)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CodesMatch(string? storedCode, string submittedCode)
        {
            if (string.IsNullOrWhiteSpace(storedCode))
                return false;

            var stored = Encoding.UTF8.GetBytes(storedCode.Trim());
            var submitted = Encoding.UTF8.GetBytes(submittedCode.Trim());

            return stored.Length == submitted.Length
                && CryptographicOperations.FixedTimeEquals(stored, submitted);
        }
    }
}
