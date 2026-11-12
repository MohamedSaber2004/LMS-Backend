namespace LMS.Application.Features.Sessions
{
    public enum BookingSlotStatus
    {
        Bookable = 0,
        InstructorNotBookable = 1,
        NoWindowsOnDate = 2,
        AllWindowsElapsed = 3,
        NoWindowFitsDuration = 4,
        AllWindowsBlocked = 5
    }

    public sealed class BookingSlotPick
    {
        public BookingSlotStatus Status { get; init; }

        public BookingBlocker Blocker { get; init; } = BookingBlocker.None;

        public int WindowCount { get; init; }

        public DateTime StartsAtUtc { get; init; }

        public DateTime EndsAtUtc { get; init; }
    }
}
