using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class SessionAttendance : BaseEntity<Guid>
    {
        public Guid SessionId { get; private set; }
        public virtual Session Session { get; private set; } = null!;

        public Guid LearnerId { get; private set; }
        public virtual Learner Learner { get; private set; } = null!;

        public AttendanceStatus Status { get; private set; }

        public DateTime? JoinedAtUtc { get; private set; }

        public DateTime? LeftAtUtc { get; private set; }

        public int? DurationMinutes { get; private set; }

        public string? Notes { get; private set; }

        private SessionAttendance() { }

        public static SessionAttendance Create(
            Guid sessionId,
            Guid learnerId,
            AttendanceStatus status = AttendanceStatus.Absent,
            DateTime? joinedAtUtc = null,
            DateTime? leftAtUtc = null,
            int? durationMinutes = null,
            string? notes = null)
        {
            return new SessionAttendance
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                LearnerId = learnerId,
                Status = status,
                JoinedAtUtc = joinedAtUtc,
                LeftAtUtc = leftAtUtc,
                DurationMinutes = durationMinutes,
                Notes = notes
            };
        }

        public void MarkAttended(DateTime? joinedAtUtc = null, DateTime? leftAtUtc = null, int? durationMinutes = null)
        {
            Status = AttendanceStatus.Present;
            JoinedAtUtc = joinedAtUtc ?? AppNow;
            LeftAtUtc = leftAtUtc;
            DurationMinutes = durationMinutes;
        }

        public void MarkAbsent(string? notes = null)
        {
            Status = AttendanceStatus.Absent;
            Notes = notes;
        }

        public void MarkLate(DateTime joinedAtUtc, string? notes = null)
        {
            Status = AttendanceStatus.Late;
            JoinedAtUtc = joinedAtUtc;
            Notes = notes;
        }

        public void MarkExcused(string reason)
        {
            Status = AttendanceStatus.Excused;
            Notes = reason;
        }
    }
}

