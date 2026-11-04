using System.Globalization;

namespace LMS.Application.Common.Helpers
{
    public static class DateTimeHelper
    {
        public const string OutputFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        public static DateTime EnsureUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => value.ToUniversalTime()
        };

        public static string FormatUtc(DateTime value)
            => EnsureUtc(value).ToString(OutputFormat, CultureInfo.InvariantCulture);

        public static string? FormatUtc(DateTime? value)
            => value.HasValue ? FormatUtc(value.Value) : null;
    }
}
