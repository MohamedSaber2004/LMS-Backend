namespace LMS.Application.Features.Availability.DTOs
{
    public class AvailabilityCalendarDto
    {
        public AvailabilityCalendarDto() { }

        public DateOnly From { get; set; }
        public DateOnly To { get; set; }

        public string? TimezoneId { get; set; }

        public IReadOnlyCollection<AvailabilityDayDto> Days { get; set; } = Array.Empty<AvailabilityDayDto>();
    }
}
