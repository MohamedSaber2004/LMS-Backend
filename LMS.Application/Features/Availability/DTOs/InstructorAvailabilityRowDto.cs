namespace LMS.Application.Features.Availability.DTOs
{
    public class InstructorAvailabilityRowDto
    {
        public InstructorAvailabilityRowDto() { }

        public Guid Id { get; set; }
        public Guid InstructorId { get; set; }
        public string InstructorFullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int Day { get; set; }
        public string DayName { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
