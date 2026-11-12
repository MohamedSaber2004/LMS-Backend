namespace LMS.Application.Features.Sessions
{
    public enum BookingSlotStatus
    {
        Bookable = 0,
        InstructorNotBookable = 1,
        NoWindowsOnDate = 2,
        TimezoneUnresolved = 3,
        StartTimeNotInTimeZone = 4,
        StartTimeElapsed = 5,
        TimeOutsideAvailability = 6,
        RequestedTimeBlocked = 7
    }

    public sealed record BookingSlotPick
    {
        public BookingSlotStatus Status { get; init; }

        public BookingBlocker Blocker { get; init; } = BookingBlocker.None;

        public int WindowCount { get; init; }

        public DateTime StartsAtUtc { get; init; }

        public DateTime EndsAtUtc { get; init; }

        public DateTime FirstWindowStartsAtUtc { get; init; }

        public DateTime LastWindowEndsAtUtc { get; init; }
    }
}
