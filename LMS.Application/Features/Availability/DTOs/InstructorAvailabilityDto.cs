namespace LMS.Application.Features.Availability.DTOs
{
    public class InstructorAvailabilityDto
    {
        public InstructorAvailabilityDto() { }

        public Guid InstructorId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Guid? CountryId { get; set; }

        public string? CountryCode { get; set; }

        public string? TimezoneId { get; set; }

        public string? CountryName { get; set; }
        public decimal HourlyRate { get; set; }

        public IReadOnlyCollection<AvailabilitySlotDto> WeeklySlots { get; set; } = Array.Empty<AvailabilitySlotDto>();
    }
}
