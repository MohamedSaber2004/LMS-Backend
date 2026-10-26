using LMS.Domain.Enums;

namespace LMS.Application.Common.Extensions
{
    public static class AppLanguageExtensions
    {
        public static string ToCode(this Language language) => language switch
        {
            Language.Ar => Language.Ar.ToString().ToLowerInvariant(),
            _ => Language.En.ToString().ToLowerInvariant()
        };

        public static Language FromCode(string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Language.En;
            }

            var parts = code.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                var langPart = part.Split(';')[0].Trim().ToLowerInvariant();

                if (langPart.StartsWith("ar"))
                {
                    return Language.Ar;
                }

                if (langPart.StartsWith("en"))
                {
                    return Language.En;
                }
            }

            return Language.En;
        }

        public static string[] GetAllCodes() => new[]
        {
            Language.En.ToCode(),
            Language.Ar.ToCode(),
        };
    }
}
