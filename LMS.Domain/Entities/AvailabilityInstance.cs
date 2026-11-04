using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class AvailabilityInstance : BaseEntity<Guid>
    {
        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public Guid? WeeklySlotId { get; private set; }

        public DateOnly Date { get; private set; }

        public DateTime StartsAtUtc { get; private set; }

        public DateTime EndsAtUtc { get; private set; }

        public bool IsOpen { get; private set; }

        public static AvailabilityInstance Create(
            Guid instructorId,
            Guid? weeklySlotId,
            DateOnly date,
            DateTime startsAtUtc,
            DateTime endsAtUtc)
        {
            return new AvailabilityInstance
            {
                InstructorId = instructorId,
                WeeklySlotId = weeklySlotId,
                Date = date,
                StartsAtUtc = startsAtUtc,
                EndsAtUtc = endsAtUtc,
                IsOpen = true
            };
        }

        public void MoveTo(DateTime startsAtUtc, DateTime endsAtUtc)
        {
            StartsAtUtc = startsAtUtc;
            EndsAtUtc = endsAtUtc;
        }

        public void Block() => IsOpen = false;

        public void Open() => IsOpen = true;

        public bool Overlaps(DateTime startsAtUtc, DateTime endsAtUtc)
            => StartsAtUtc < endsAtUtc && EndsAtUtc > startsAtUtc;
    }
}
