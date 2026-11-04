using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class SessionBooking : BaseEntity<Guid>
    {
        public Guid SessionId { get; private set; }
        public virtual Session Session { get; private set; } = null!;

        public Guid LearnerId { get; private set; }
        public virtual Learner Learner { get; private set; } = null!;

        public BookingStatus Status { get; private set; }

        public Guid? CancelledByUserId { get; private set; }
        public string? CancelReason { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public static SessionBooking Create(Guid sessionId, Guid learnerId)
        {
            return new SessionBooking
            {
                SessionId = sessionId,
                LearnerId = learnerId,
                Status = BookingStatus.Pending
            };
        }

        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
        }

        public void Decline(Guid actorUserId)
        {
            Status = BookingStatus.Declined;
            CancelledByUserId = actorUserId;
            CancelledAt = AppNow;
        }

        public void Cancel(Guid? actorUserId, string? reason)
        {
            Status = BookingStatus.Cancelled;
            CancelledByUserId = actorUserId;
            CancelReason = reason;
            CancelledAt = AppNow;
        }

        public bool HoldsSeat()
        {
            return Status == BookingStatus.Pending || Status == BookingStatus.Confirmed;
        }
    }
}
