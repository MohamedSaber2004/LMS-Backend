namespace LMS.Application.Features.Availability.DTOs
{
    public class AvailabilitySlotDto
    {
        public AvailabilitySlotDto() { }

        public Guid Id { get; set; }
        public Guid InstructorId { get; set; }

        /// <summary>
        /// Day of week as stored: 0 = Sunday through 6 = Saturday.
        /// </summary>
        public int Day { get; set; }

        public string DayName { get; set; } = string.Empty;

        /// <summary>
        /// Instructor local wall-clock time. No timezone conversion is applied by the API.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
