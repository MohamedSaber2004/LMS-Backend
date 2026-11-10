using LMS.Domain.Common;
using LMS.Domain.Enums;

namespace LMS.Domain.Entities
{
    public class Session : BaseEntity<Guid>
    {
        public Guid InstructorId { get; private set; }
        public virtual Instructor Instructor { get; private set; } = null!;

        public Guid? LessonId { get; private set; }
        public virtual Lesson? Lesson { get; private set; }

        public string? Title { get; private set; }

        public SessionType Type { get; private set; }

        public DateTime StartsAtUtc { get; private set; }

        public DateTime EndsAtUtc { get; private set; }

        public SessionStatus Status { get; private set; }

        public decimal Price { get; private set; }
        public int DurationMinutes { get; private set; } = 50;
        public bool IsTrial { get; private set; }
        public decimal HourlyRateSnapshot { get; private set; }
        public decimal CommissionRateSnapshot { get; private set; }
        public bool IsAttendanceConfirmed { get; private set; }
        public DateTime? AttendanceConfirmedAtUtc { get; private set; }

        public int SeatCapacity { get; private set; }

        public int SeatsBooked { get; private set; }

        public Guid? CancelledByUserId { get; private set; }
        public string? CancelReason { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public virtual ICollection<SessionBooking> Bookings { get; private set; } = new List<SessionBooking>();
        public virtual ICollection<SessionAttendance> Attendances { get; private set; } = new List<SessionAttendance>();
        public virtual ICollection<RescheduleRequest> RescheduleRequests { get; private set; } = new List<RescheduleRequest>();
        public virtual LiveRoom? LiveRoom { get; private set; }

        public static Session Create(
            Guid instructorId,
            SessionType type,
            DateTime startsAtUtc,
            DateTime endsAtUtc,
            decimal price,
            string? title = null,
            int seatCapacity = 1,
            Guid? lessonId = null,
            int durationMinutes = 50,
            bool isTrial = false,
            decimal hourlyRateSnapshot = 0m,
            decimal commissionRateSnapshot = 0m)
        {
            return new Session
            {
                InstructorId = instructorId,
                LessonId = lessonId,
                Type = type,
                StartsAtUtc = startsAtUtc,
                EndsAtUtc = endsAtUtc,
                Price = price,
                Title = title,
                SeatCapacity = type == SessionType.OneToOne ? 1 : seatCapacity,
                Status = SessionStatus.Pending,
                SeatsBooked = 0,
                DurationMinutes = durationMinutes,
                IsTrial = isTrial,
                HourlyRateSnapshot = hourlyRateSnapshot,
                CommissionRateSnapshot = commissionRateSnapshot
            };
        }

        public void ConfirmAttendance(DateTime confirmedAtUtc)
        {
            IsAttendanceConfirmed = true;
            AttendanceConfirmedAtUtc = confirmedAtUtc;
            Status = SessionStatus.Completed;
        }

        public void AssignLesson(Guid lessonId)
        {
            LessonId = lessonId;
        }

        public void DetachLesson()
        {
            LessonId = null;
        }

        public void Confirm()
        {
            Status = SessionStatus.Confirmed;
        }

        public void Complete()
        {
            Status = SessionStatus.Completed;
        }

        public void MarkNoShow()
        {
            Status = SessionStatus.NoShow;
        }

        public void Cancel(Guid? cancelledByUserId, string? reason)
        {
            Status = SessionStatus.Cancelled;
            CancelledByUserId = cancelledByUserId;
            CancelReason = reason;
            CancelledAt = AppNow;
        }

        public void MoveTo(DateTime startsAtUtc, DateTime endsAtUtc)
        {
            StartsAtUtc = startsAtUtc;
            EndsAtUtc = endsAtUtc;
        }

        public void ReserveSeat()
        {
            SeatsBooked++;
        }

        public void ReleaseSeat()
        {
            SeatsBooked = SeatsBooked > 0 ? SeatsBooked - 1 : 0;
        }

        public bool HasSeatAvailable()
        {
            return SeatsBooked < SeatCapacity;
        }

        public bool Overlaps(DateTime startsAtUtc, DateTime endsAtUtc)
        {
            return StartsAtUtc < endsAtUtc && EndsAtUtc > startsAtUtc;
        }
    }
}
