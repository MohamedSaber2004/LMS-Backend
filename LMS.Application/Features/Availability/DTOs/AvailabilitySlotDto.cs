namespace LMS.Application.Features.Availability.DTOs
{
    public class AvailabilitySlotDto
    {
        public AvailabilitySlotDto() { }

        public Guid Id { get; set; }
        public Guid InstructorId { get; set; }

        public int Day { get; set; }

        public string DayName { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
