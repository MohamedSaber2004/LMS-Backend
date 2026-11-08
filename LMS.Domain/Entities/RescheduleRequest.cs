using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class RescheduleRequest : BaseEntity<Guid>
    {
        public Guid SessionId { get; private set; }
        public virtual Session Session { get; private set; } = null!;

        public Guid RequestedByUserId { get; private set; }

        public UserType RequestedByType { get; private set; }

        public DateTime FromUtc { get; private set; }

        public DateTime ToUtc { get; private set; }

        public RescheduleRequestStatus Status { get; private set; }

        public Guid? RespondedByUserId { get; private set; }
        public DateTime? RespondedAt { get; private set; }

        public static RescheduleRequest Create(
            Guid sessionId,
            Guid requestedByUserId,
            UserType requestedByType,
            DateTime fromUtc,
            DateTime toUtc)
        {
            return new RescheduleRequest
            {
                SessionId = sessionId,
                RequestedByUserId = requestedByUserId,
                RequestedByType = requestedByType,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                Status = RescheduleRequestStatus.Pending
            };
        }

        public void Approve(Guid respondedByUserId)
        {
            Status = RescheduleRequestStatus.Approved;
            RespondedByUserId = respondedByUserId;
            RespondedAt = AppNow;
        }

        public void Decline(Guid respondedByUserId)
        {
            Status = RescheduleRequestStatus.Declined;
            RespondedByUserId = respondedByUserId;
            RespondedAt = AppNow;
        }

        public void Withdraw(Guid actorUserId)
        {
            Status = RescheduleRequestStatus.Withdrawn;
            RespondedByUserId = actorUserId;
            RespondedAt = AppNow;
        }

        public bool IsRequestedByInstructor()
        {
            return RequestedByType == UserType.Instructor;
        }
    }
}
