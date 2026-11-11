using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class LearnerPackage : BaseEntity<Guid>
    {
        public Guid LearnerId { get; private set; }
        public virtual Learner Learner { get; private set; } = null!;

        public Guid? InstructorId { get; private set; }
        public virtual Instructor? Instructor { get; private set; }

        public string PackageName { get; private set; } = null!;
        public decimal TotalHours { get; private set; }
        public decimal RemainingHours { get; private set; }
        public decimal HourlyRateSnapshot { get; private set; }
        public decimal TotalPrice { get; private set; }

        public static LearnerPackage Create(
            Guid learnerId,
            string packageName,
            decimal totalHours,
            decimal hourlyRateSnapshot = 0m,
            Guid? instructorId = null,
            decimal? totalPrice = null)
        {
            return new LearnerPackage
            {
                LearnerId = learnerId,
                InstructorId = instructorId,
                PackageName = packageName,
                TotalHours = totalHours,
                RemainingHours = totalHours,
                HourlyRateSnapshot = hourlyRateSnapshot,
                TotalPrice = totalPrice ?? Math.Round(totalHours * hourlyRateSnapshot, 2)
            };
        }

        public static LearnerPackage Create(
            Guid learnerId,
            Guid? instructorId,
            string packageName,
            decimal totalHours,
            decimal hourlyRateSnapshot)
        {
            return Create(learnerId, packageName, totalHours, hourlyRateSnapshot, instructorId);
        }

        public void DeductHours(decimal hours)
        {
            RemainingHours = Math.Max(0, RemainingHours - hours);
            if (RemainingHours <= 0)
            {
                Deactive();
            }
        }

        public void RefundHours(decimal hours)
        {
            RemainingHours += hours;
            Active();
        }

        public void AdjustHours(decimal newRemainingHours)
        {
            RemainingHours = Math.Max(0, newRemainingHours);
            if (RemainingHours > 0)
            {
                Active();
            }
            else
            {
                Deactive();
            }
        }
    }
}
