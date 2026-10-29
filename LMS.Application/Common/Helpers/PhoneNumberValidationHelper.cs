namespace LMS.Application.Common.Helpers
{
    public static class PhoneNumberValidationHelper
    {
        private const int MaxLength = 30;
        private const int MinDigits = 6;
        private const int MaxDigits = 15;

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
