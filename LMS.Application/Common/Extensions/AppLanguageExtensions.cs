using LMS.Domain.Enums;

namespace LMS.Application.Common.Extensions
{
    public static class AppLanguageExtensions
    {
        public static string ToCode(this Language language) => Language.En.ToString().ToLowerInvariant();

        public static Language FromCode(string? code) => Language.En;

        public static string[] GetAllCodes() => new[]
        {
            Language.En.ToCode()
        };
    }
}
