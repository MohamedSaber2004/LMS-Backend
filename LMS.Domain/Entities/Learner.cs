using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class Learner : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public virtual ApplicationUser User { get; private set; } = null!;

        public decimal? HourlyRate { get; private set; }

        public virtual ICollection<SessionAttendance> Attendances { get; private set; } = new List<SessionAttendance>();
        public virtual ICollection<LearnerPackage> Packages { get; private set; } = new List<LearnerPackage>();

        public static Learner Create(Guid userId, decimal? hourlyRate = null)
        {
            return new Learner
            {
                UserId = userId,
                HourlyRate = hourlyRate
            };
        }

        public void UpdateHourlyRate(decimal? hourlyRate)
        {
            HourlyRate = hourlyRate;
        }
    }
}
