using LMS.Domain.Enums;

namespace LMS.Application.Features.Sessions.DTOs
{
    public class TeacherRescheduleRequestDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string? SessionTitle { get; set; }
        public DateTime CurrentStartsAtUtc { get; set; }
        public DateTime CurrentEndsAtUtc { get; set; }
        public DateTime ProposedStartsAtUtc { get; set; }
        public DateTime ProposedEndsAtUtc { get; set; }
        public RescheduleRequestStatus Status { get; set; }
        public UserType RequestedByType { get; set; }
        public bool IsRequestedByMe { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<SessionParticipantSummaryDto> Participants { get; set; } = [];
    }

    public class SessionParticipantSummaryDto
    {
        public Guid LearnerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}
