using LMS.Application.Localization;

namespace LMS.Application.Common.Helpers
{
    public static class AuthFailureReasons
    {
        public const string ItemsKey = "LMS.AuthFailureReason";
        public const string Expired = "Expired";
        public const string Revoked = "Revoked";
        public const string MissingClaims = "MissingClaims";
        public const string Invalid = "Invalid";

        private static readonly Dictionary<string, string> LocalizationKeysByReason = new()
        {
            [Expired] = LocalizationKeys.ExceptionMessages.TokenExpired,
            [Revoked] = LocalizationKeys.ExceptionMessages.TokenRevoked,
            [MissingClaims] = LocalizationKeys.ExceptionMessages.TokenMissingClaims,
            [Invalid] = LocalizationKeys.ExceptionMessages.TokenInvalid,
        };

        public static string GetLocalizationKey(string? reason)
            => reason is not null && LocalizationKeysByReason.TryGetValue(reason, out var localizationKey)
                ? localizationKey
                : LocalizationKeys.ExceptionMessages.Unauthorized;
    }
}
