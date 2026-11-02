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

        public static InstructorApplication Create(Guid instructorId)
        {
            return new InstructorApplication
            {
                InstructorId = instructorId
            };
        }

        public void Approve(Guid reviewedByUserId, decimal hourlyRate, string? reviewNotes)
        {
            Status = InstructorApplicationStatus.Approved;
            HourlyRate = hourlyRate;
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
    }
}
