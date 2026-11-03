namespace LMS.Application.Features.Availability.DTOs
{
    /// <summary>
    /// Availability of a single instructor. Times are the instructor's own local wall-clock
    /// values; the caller resolves the timezone from the supplied country.
    /// </summary>
    public class InstructorAvailabilityDto
    {
        public InstructorAvailabilityDto() { }

        public Guid InstructorId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Guid? CountryId { get; set; }

        /// <summary>
        /// Country dialling code (for example "+20"), used by the caller to resolve the timezone.
        /// </summary>
        public string? CountryCode { get; set; }

        public string? CountryName { get; set; }
        public decimal HourlyRate { get; set; }

        public IReadOnlyCollection<AvailabilitySlotDto> WeeklySlots { get; set; } = Array.Empty<AvailabilitySlotDto>();
    }
}
