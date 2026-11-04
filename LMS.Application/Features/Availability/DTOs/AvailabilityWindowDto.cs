namespace LMS.Application.Features.Availability.DTOs
{
    public class AvailabilityWindowDto
    {
        public AvailabilityWindowDto() { }

        public Guid Id { get; set; }
        public DateTime StartsAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }
    }
}
