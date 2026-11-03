using LMS.Domain.Common;

namespace LMS.Domain.Entities
{
    public class InstructorAvailabilitySlot : BaseEntity<Guid>
    {
        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public DayOfWeek Day { get; private set; }

        public TimeSpan StartTime { get; private set; }

        public TimeSpan EndTime { get; private set; }

        public static InstructorAvailabilitySlot Create(
            Guid instructorId,
            DayOfWeek day,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            return new InstructorAvailabilitySlot
            {
                InstructorId = instructorId,
                Day = day,
                StartTime = startTime,
                EndTime = endTime
            };
        }

        public void Update(DayOfWeek day, TimeSpan startTime, TimeSpan endTime)
        {
            Day = day;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
