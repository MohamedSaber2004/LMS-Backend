using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class InstructorApplication : BaseEntity<Guid>
    {
        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public InstructorApplicationStatus Status { get; private set; } = InstructorApplicationStatus.Pending;
        public decimal HourlyRate { get; private set; }
        public Guid? ReviewedByUserId { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public string? ReviewNotes { get; private set; }

        public static InstructorApplication Create(Guid instructorId, decimal hourlyRate)
        {
            return new InstructorApplication
            {
                InstructorId = instructorId,
                HourlyRate = hourlyRate
            };
        }

        public void Approve(Guid reviewedByUserId, string? reviewNotes)
        {
            Status = InstructorApplicationStatus.Approved;
            ReviewedByUserId = reviewedByUserId;
            ReviewedAt = DateTime.UtcNow;
            ReviewNotes = reviewNotes;
            MarkAsUpdated(reviewedByUserId.ToString());
        }

        public void Reject(Guid reviewedByUserId, string reviewNotes)
        {
            Status = InstructorApplicationStatus.Rejected;
            ReviewedByUserId = reviewedByUserId;
            ReviewedAt = DateTime.UtcNow;
            ReviewNotes = reviewNotes;
            MarkAsUpdated(reviewedByUserId.ToString());
        }

        public void Resubmit(Guid actorUserId, decimal hourlyRate)
        {
            Status = InstructorApplicationStatus.Pending;
            HourlyRate = hourlyRate;
            ReviewedByUserId = null;
            ReviewedAt = null;
            ReviewNotes = null;
            MarkAsUpdated(actorUserId.ToString());
        }
    }
}
