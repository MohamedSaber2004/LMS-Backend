using System.Globalization;

namespace LMS.Application.Common.Helpers
{
    public static class DateTimeHelper
    {
        public const string OutputFormat = "yyyy-MM-dd HH:mm:ss";

        public static DateTime EnsureUtc(DateTime value)
            => value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value.ToUniversalTime(), DateTimeKind.Utc);

        public static string FormatUtc(DateTime value)
            => EnsureUtc(value).ToString(OutputFormat, CultureInfo.InvariantCulture);

        public static string? FormatUtc(DateTime? value)
            => value.HasValue ? FormatUtc(value.Value) : null;
    }
}
