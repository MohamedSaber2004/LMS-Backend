namespace LMS.Application.Features.Sessions.DTOs
{
    public class SessionDetailDto
    {
        public SessionDto Session { get; set; } = new();

        /// <summary>Empty for a learner caller: the roster of who is in the session belongs to the instructor.</summary>
        public IReadOnlyCollection<SessionBookingDto> Bookings { get; set; } = Array.Empty<SessionBookingDto>();

        public IReadOnlyCollection<RescheduleRequestDto> RescheduleRequests { get; set; }
            = Array.Empty<RescheduleRequestDto>();
    }
}
