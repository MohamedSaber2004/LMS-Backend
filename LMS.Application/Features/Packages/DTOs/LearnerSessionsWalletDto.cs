namespace LMS.Application.Features.Packages.DTOs
{
    public class LearnerSessionsWalletDto
    {
        public decimal TotalSessions { get; set; }
        public decimal RemainingSessions { get; set; }
        public decimal ConsumedSessions => Math.Max(0, TotalSessions - RemainingSessions);
        public int RemainingFullSessions => (int)Math.Floor(RemainingSessions / 1.0m);
        public int RemainingHalfSessions => (int)Math.Floor(RemainingSessions / 0.5m);
        public bool HasActivePlan => RemainingSessions > 0;
        public string? ActivePlanName { get; set; }
        public bool HasFreeTrialAvailable { get; set; }
        public IReadOnlyCollection<LearnerPackageDto> Packages { get; set; } = Array.Empty<LearnerPackageDto>();
    }
}

