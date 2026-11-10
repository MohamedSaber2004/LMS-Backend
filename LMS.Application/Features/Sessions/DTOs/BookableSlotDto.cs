namespace LMS.Application.Features.Sessions.DTOs
{
    /// <summary>
    /// One dated window the learner can actually book: an open availability row with no live session
    /// inside it yet.
    /// </summary>
    public class BookableSlotDto
    {
        public Guid AvailabilityInstanceId { get; set; }

        public DateOnly Date { get; set; }

        public DateTime StartsAtUtc { get; set; }

        public DateTime EndsAtUtc { get; set; }

        public DateTime? LocalStartsAt { get; set; }

        public DateTime? LocalEndsAt { get; set; }

        public DateOnly? LocalDate { get; set; }

        public string? TimeZoneId { get; set; }
    }
}
