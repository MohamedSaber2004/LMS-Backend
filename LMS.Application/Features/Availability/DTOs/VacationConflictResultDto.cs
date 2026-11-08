using LMS.Domain.Enums;

namespace LMS.Application.Features.Availability.DTOs
{
    public class VacationConflictResultDto
    {
        public bool HasConflicts => ConflictCount > 0;
        public int ConflictCount => ConflictedSessions.Count;
        public bool CanProceed => ConflictCount == 0;
        public IReadOnlyCollection<VacationConflictedSessionDto> ConflictedSessions { get; set; } = [];
    }

    public class VacationConflictedSessionDto
    {
        public Guid SessionId { get; set; }
        public string? Title { get; set; }
        public DateTime StartsAtUtc { get; set; }
        public DateTime EndsAtUtc { get; set; }
        public SessionStatus Status { get; set; }
        public SessionType Type { get; set; }
        public int SeatsBooked { get; set; }
        public int SeatCapacity { get; set; }
        public IReadOnlyCollection<string> LearnerNames { get; set; } = [];
    }
}
