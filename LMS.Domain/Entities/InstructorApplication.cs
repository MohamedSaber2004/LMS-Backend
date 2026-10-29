using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class InstructorApplication : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public virtual ApplicationUser User { get; private set; } = null!;

        public string Specialization { get; private set; } = null!;
        public string Bio { get; private set; } = null!;
        public int YearsOfExperience { get; private set; }
        public string? LinkedInUrl { get; private set; }

        public InstructorApplicationStatus Status { get; private set; } = InstructorApplicationStatus.Pending;
        public Guid? ReviewedByUserId { get; private set; }
        public DateTime? ReviewedAt { get; private set; }
        public string? ReviewNotes { get; private set; }

        public virtual ICollection<InstructorCertificate> Certificates { get; private set; } = new List<InstructorCertificate>();

        public static InstructorApplication Create(
            Guid userId,
            string specialization,
            string bio,
            int yearsOfExperience,
            string? linkedInUrl)
        {
            return new InstructorApplication
            {
                UserId = userId,
                Specialization = specialization,
                Bio = bio,
                YearsOfExperience = yearsOfExperience,
                LinkedInUrl = linkedInUrl
            };
        }

        public void AddCertificate(InstructorCertificate certificate)
        {
            Certificates.Add(certificate);
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
    }
}
