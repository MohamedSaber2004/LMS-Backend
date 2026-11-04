using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class RescheduleRequestDto
    {
        public Guid Id { get; set; }

        public Guid SessionId { get; set; }

        public Guid RequestedByUserId { get; set; }

        public UserType RequestedByType { get; set; }

        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }

        public RescheduleRequestStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
