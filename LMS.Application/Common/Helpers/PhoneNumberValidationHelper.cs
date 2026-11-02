namespace LMS.Application.Common.Helpers
{
    public static class PhoneNumberValidationHelper
    {
        private const int MaxLength = 30;
        private const int MinDigits = 6;
        private const int MaxDigits = 15;
        private const int MinE164Digits = 8;
        private const int MaxE164Digits = 15;
        private const string E164Separators = " -()./\t";

        public static string NormalizeE164(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return string.Empty;
            }

            var cleaned = new string(phone
                .Where(character => !E164Separators.Contains(character))
                .ToArray());

            return cleaned.StartsWith("00", StringComparison.Ordinal)
                ? "+" + cleaned[2..]
                : cleaned;
        }

        public static bool IsValidE164(string? phone)
        {
            var normalized = NormalizeE164(phone);

            if (normalized.Length < 2 || normalized[0] != '+')
            {
                return false;
            }

            var digits = normalized.Skip(1).Count(char.IsDigit);
            return digits >= MinE164Digits
                && digits <= MaxE164Digits
                && normalized.Length - 1 == digits;
        }

        public static bool MatchesCountryDialCode(string? phone, string? countryDialCode)
        {
            var dialDigits = new string((countryDialCode ?? string.Empty).Where(char.IsDigit).ToArray());

            if (dialDigits.Length == 0)
            {
                return true;
            }

            var phoneDigits = new string(NormalizeE164(phone).Where(char.IsDigit).ToArray());

            return phoneDigits.Length > dialDigits.Length
                && phoneDigits.StartsWith(dialDigits, StringComparison.Ordinal);
        }

        public static bool IsValid(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            var value = phone.Trim();

            if (value.Length > MaxLength)
            {
                return false;
            }

            var plusIndex = value.IndexOf('+');
            if (plusIndex > 0 || (plusIndex == 0 && value.IndexOf('+', 1) >= 0))
            {
                return false;
            }

            foreach (var character in value)
            {
                if (!char.IsDigit(character)
                    && !char.IsWhiteSpace(character)
                    && character != '+'
                    && character != '-'
                    && character != '('
                    && character != ')'
                    && character != '.'
                    && character != '/')
                {
                    return false;
                }
            }

            var digitCount = value.Count(char.IsDigit);
            return digitCount >= MinDigits && digitCount <= MaxDigits;
        }
    }
}
