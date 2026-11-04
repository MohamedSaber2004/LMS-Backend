namespace LMS.Application.Features.Availability.DTOs
{
    public class AvailabilityDayDto
    {
        public AvailabilityDayDto() { }

        public DateOnly Date { get; set; }

        public IReadOnlyCollection<AvailabilityWindowDto> Available { get; set; } = Array.Empty<AvailabilityWindowDto>();

        public IReadOnlyCollection<AvailabilityWindowDto> Blocked { get; set; } = Array.Empty<AvailabilityWindowDto>();

        public IReadOnlyCollection<AvailabilityWindowDto> Booked { get; set; } = Array.Empty<AvailabilityWindowDto>();
    }
}
