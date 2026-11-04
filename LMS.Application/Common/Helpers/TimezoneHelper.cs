namespace LMS.Application.Common.Helpers
{
    public static class TimezoneHelper
    {
        public const int MaxLength = 64;

        public static bool IsValidIanaId(string? id)
        {
            return !string.IsNullOrWhiteSpace(id)
                && id.Length <= MaxLength
                && TimeZoneInfo.TryFindSystemTimeZoneById(id.Trim(), out _);
        }

        public static bool TryResolve(string? id, out TimeZoneInfo? timeZone)
        {
            timeZone = null;

            if (string.IsNullOrWhiteSpace(id) || id.Length > MaxLength)
            {
                return false;
            }

            return TimeZoneInfo.TryFindSystemTimeZoneById(id.Trim(), out timeZone) && timeZone is not null;
        }

        public static bool TryToUtc(DateOnly date, TimeSpan time, TimeZoneInfo zone, out DateTime utcInstant)
        {
            utcInstant = default;

            var wallClock = DateTime.SpecifyKind(
                date.ToDateTime(TimeOnly.FromTimeSpan(time)),
                DateTimeKind.Unspecified);

            try
            {
                utcInstant = TimeZoneInfo.ConvertTimeToUtc(wallClock, zone);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static void ToWallClock(DateTime utcInstant, TimeZoneInfo zone, out DateOnly date, out TimeSpan time)
        {
            var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcInstant, DateTimeKind.Utc), zone);
            date = DateOnly.FromDateTime(local);
            time = local.TimeOfDay;
        }

        public static bool TryToUtcDayBounds(DateOnly date, TimeZoneInfo zone, out DateTime startsAtUtc, out DateTime endsAtUtc)
        {
            startsAtUtc = default;
            endsAtUtc = default;

            return TryToUtc(date, TimeSpan.Zero, zone, out startsAtUtc)
                && TryToUtc(date.AddDays(1), TimeSpan.Zero, zone, out endsAtUtc);
        }

        public static DateOnly LocalToday(TimeZoneInfo zone)
        {
            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
        }
    }
}
