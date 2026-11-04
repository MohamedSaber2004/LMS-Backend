namespace LMS.Application.Features.Availability.DTOs
{
    public class MyAvailabilityDto
    {
        public MyAvailabilityDto() { }

        public Guid InstructorId { get; set; }

        public string? TimezoneId { get; set; }

        public bool VacationMode { get; set; }

        public IReadOnlyCollection<AvailabilitySlotDto> WeeklySlots { get; set; } = Array.Empty<AvailabilitySlotDto>();
    }
}
