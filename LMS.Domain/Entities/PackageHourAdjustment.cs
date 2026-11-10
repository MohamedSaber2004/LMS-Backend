using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class PackageHourAdjustment : BaseEntity<Guid>
    {
        public Guid PackageId { get; private set; }
        public virtual LearnerPackage Package { get; private set; } = null!;

        public decimal OldRemainingHours { get; private set; }
        public decimal NewRemainingHours { get; private set; }
        public string Reason { get; private set; } = null!;

        public static PackageHourAdjustment Create(
            Guid packageId,
            decimal oldRemainingHours,
            decimal newRemainingHours,
            string reason)
        {
            return new PackageHourAdjustment
            {
                PackageId = packageId,
                OldRemainingHours = oldRemainingHours,
                NewRemainingHours = newRemainingHours,
                Reason = reason
            };
        }
    }
}
